using Artigo.API.GraphQL.Mutations;
using Artigo.API.GraphQL.Queries;
using Artigo.API.GraphQL.Types;
using Artigo.DbContext.Data;
using Artigo.DbContext.Mappers;
using Artigo.DbContext.Repositories;
using Artigo.Intf.Enums;
using Artigo.Intf.Interfaces;
using Artigo.Server.Mappers;
using Artigo.Server.Services;
using AutoMapper;
using HotChocolate.Data.MongoDb;
using HotChocolate.DataLoader; // Needed for IBatchScheduler
using HotChocolate.Execution.Configuration; // Needed for AddGraphQLServer
using HotChocolate.Types;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MongoDB.Driver;
using System.Reflection; // Added for completeness, though often implicit
using Artigo.Intf.Entities; // Added for Entities like EditorialTeam, ContribuicaoEditorial
using GreenDonut; // Added for DataLoaderOptions
using Artigo.API.GraphQL.DataLoaders; // Added for DataLoaders defined in separate files


var builder = WebApplication.CreateBuilder(args);

// =========================================================================
// 1. CONFIGURAÇÃO DO MONGODB
// =========================================================================

// A. Singleton do IMongoClient (Conexão Centralizada)
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("MongoDb")
        ?? "mongodb://localhost:27017";
    return new MongoClient(connectionString);
});

// B. Implementação do contexto de dados (IMongoDbContext)
builder.Services.AddSingleton<IMongoDbContext>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var databaseName = builder.Configuration["MongoDb:DatabaseName"] ?? "MagazineArtigoDB";
    return new MongoDbContext(client, databaseName);
});

// C. Registra a classe de Contexto para injeção
builder.Services.AddSingleton<MongoDbContext>();


// =========================================================================
// 2. CONFIGURAÇÃO DO AUTOMAPPER
// =========================================================================

// FIX: Usamos a inicialização manual e registro explícito para evitar ambiguidades do compilador.
var mapperConfig = new AutoMapper.MapperConfiguration(cfg =>
{
    // Usa o método AddProfile para carregar os perfis de ambas as assemblies.
    cfg.AddProfile<ArtigoMappingProfile>();
    cfg.AddProfile<PersistenceMappingProfile>();
});

mapperConfig.AssertConfigurationIsValid();

// Registra a instância IMapper como Singleton no DI.
builder.Services.AddSingleton<AutoMapper.IMapper>(sp => mapperConfig.CreateMapper());


// =========================================================================
// 3. REGISTRO DE REPOSITORIES E SERVICES (DI)
// =========================================================================

// Repositories (Scoped Lifetime for safety)
builder.Services.AddScoped<IArtigoRepository, ArtigoRepository>();
builder.Services.AddScoped<IAutorRepository, AutorRepository>();
builder.Services.AddScoped<IEditorialRepository, EditorialRepository>();
builder.Services.AddScoped<IArtigoHistoryRepository, ArtigoHistoryRepository>();
builder.Services.AddScoped<IInteractionRepository, InteractionRepository>();
builder.Services.AddScoped<IPendingRepository, PendingRepository>();
builder.Services.AddScoped<IStaffRepository, StaffRepository>();
builder.Services.AddScoped<IVolumeRepository, VolumeRepository>();

// Services (Application Layer)
builder.Services.AddScoped<IArtigoService, ArtigoService>();


// =========================================================================
// 4. CONFIGURAÇÃO DO HOT CHOCOLATE (GraphQL)
// =========================================================================

var graphQLServer = builder.Services.AddGraphQLServer()
    .AddAuthorizationCore() // FIX: Correct method for GraphQL authorization logic
    .AddFiltering()
    .AddSorting()
    .AddProjections()
    .AddQueryType<ArtigoQueries>()
    .AddMutationType<ArtigoMutations>()

    // Mapeia todos os tipos definidos
    .AddType<ArtigoType>()
    .AddType<EditorialType>()
    .AddType<AutorType>()
    .AddType<VolumeType>()
    .AddType<Artigo.API.GraphQL.Types.InteractionType>()
    .AddType<PendingType>()
    .AddType<StaffType>()
    .AddType<ArtigoHistoryType>()

    // Mapeia tipos embutidos (necessário para que HotChocolate os encontre)
    .AddType<EditorialTeamType>()
    .AddType<ContribuicaoEditorialType>()

    // Mapeia Enums
    .BindRuntimeType<ArtigoStatus, EnumType<ArtigoStatus>>()
    .BindRuntimeType<ArtigoTipo, EnumType<ArtigoTipo>>()
    .BindRuntimeType<EditorialPosition, EnumType<EditorialPosition>>()
    .BindRuntimeType<ContribuicaoRole, EnumType<ContribuicaoRole>>()
    .BindRuntimeType<Artigo.Intf.Enums.InteractionType, EnumType<Artigo.Intf.Enums.InteractionType>>()
    .BindRuntimeType<PendingStatus, EnumType<PendingStatus>>()
    .BindRuntimeType<TargetEntityType, EnumType<TargetEntityType>>()
    .BindRuntimeType<JobRole, EnumType<JobRole>>()
    .BindRuntimeType<VolumeMes, EnumType<VolumeMes>>()

    // Configura DataLoaders (definidos em Artigo.API.GraphQL.DataLoaders)
    // Os DataLoaders sem dependências complexas podem ser registrados diretamente
    .AddDataLoader<EditorialDataLoader>()
    .AddDataLoader<VolumeDataLoader>()
    .AddDataLoader<AutorDataLoader>()
    .AddDataLoader<InteractionDataLoader>()
    .AddDataLoader<CurrentHistoryContentDataLoader>()
    .AddDataLoader<ArtigoHistoryGroupedDataLoader>()
    .AddDataLoader<InteractionRepliesDataLoader>()

    // DataLoaders com dependências complexas (ArtigoGroupedDataLoader)
    .AddDataLoader(sp =>
        new ArtigoGroupedDataLoader(
            sp.GetRequiredService<GreenDonut.IBatchScheduler>(), // Necessário para evitar ambiguidade com HotChocolate
            sp.GetRequiredService<IArtigoRepository>(),
            sp.GetRequiredService<AutoMapper.IMapper>()
        )
    )

    // Configuração para uso com MongoDB
    .AddMongoDbProjections()
    .AddMongoDbFiltering()
    .AddMongoDbSorting();


// =========================================================================
// 5. CONFIGURAÇÃO DE AUTENTICAÇÃO E ROTEAMENTO
// =========================================================================

// FIX: using Microsoft.AspNetCore.Authentication.JwtBearer; is implicitly included
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["UsuarioAPI:Authority"];
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization();


var app = builder.Build();

// =========================================================================
// 6. MIDDLEWARE PIPELINE
// =========================================================================

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapGraphQL();
    // Opcional: endpoints.MapBananaCakePop("/graphql-ui");
});


app.Run();


// =========================================================================
// Placeholder/Helper Definitions (Re-introduced for compilation consistency)
// =========================================================================

// Esta classe é necessária para o HotChocolate encontrar a definição para injeção.
public class ArtigoGroupedDataLoader : GroupedDataLoader<string, Artigo.Server.DTOs.ArtigoDTO>
{
    private readonly IArtigoRepository _artigoRepository;
    private readonly AutoMapper.IMapper _mapper;

    public ArtigoGroupedDataLoader(
        GreenDonut.IBatchScheduler scheduler,
        IArtigoRepository artigoRepository,
        AutoMapper.IMapper mapper)
        : base(scheduler, new GreenDonut.DataLoaderOptions())
    {
        _artigoRepository = artigoRepository;
        _mapper = mapper;
    }

    protected override async Task<ILookup<string, Artigo.Server.DTOs.ArtigoDTO>> LoadGroupedBatchAsync(IReadOnlyList<string> keys, CancellationToken cancellationToken)
    {
        var artigos = await _artigoRepository.GetByIdsAsync(keys.ToList());
        var dtos = _mapper.Map<IReadOnlyList<Artigo.Server.DTOs.ArtigoDTO>>(artigos);
        return dtos.ToLookup(a => a.Id, a => a);
    }
}
// Placeholder for EditorialTeamType (needed if not defined as nested in EditorialType.cs)
public class EditorialTeamType : HotChocolate.Types.ObjectType<Artigo.Intf.Entities.EditorialTeam>
{
    protected override void Configure(HotChocolate.Types.IObjectTypeDescriptor<Artigo.Intf.Entities.EditorialTeam> descriptor)
    {
        descriptor.Field(f => f.EditorId);
    }
}
// Placeholder for ContribuicaoEditorialType (needed if not defined as nested in AutorType.cs)
public class ContribuicaoEditorialType : HotChocolate.Types.ObjectType<Artigo.Intf.Entities.ContribuicaoEditorial>
{
    protected override void Configure(HotChocolate.Types.IObjectTypeDescriptor<Artigo.Intf.Entities.ContribuicaoEditorial> descriptor)
    {
        descriptor.Field(f => f.ArtigoId);
    }
}