using Artigo.API.GraphQL.Mutations;
using Artigo.API.GraphQL.Queries;
using Artigo.API.GraphQL.Types;
using Artigo.DbContext.Data;
using Artigo.DbContext.Mappers;
using Artigo.DbContext.PersistenceModels;
using Artigo.DbContext.Repositories;
using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using Artigo.Intf.Interfaces;
using Artigo.Server.Mappers;
using Artigo.Server.Services;
using AutoMapper;
using HotChocolate.Data;
using HotChocolate.Execution.Configuration;
using HotChocolate.Types;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// =========================================================================
// 1. CONFIGURAÇÃO DO MONGODB (Database Settings)
// (Unchanged from last version)
// =========================================================================

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("MongoDb")
        ?? "mongodb://localhost:27017";
    return new MongoClient(connectionString);
});

builder.Services.AddSingleton<IMongoDbContext>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var databaseName = builder.Configuration["MongoDb:DatabaseName"] ?? "MagazineArtigoDB";
    return new MongoDbContext(client, databaseName);
});

builder.Services.AddSingleton<MongoDbContext>();


// =========================================================================
// 2. CONFIGURAÇÃO DO AUTOMAPPER
// =========================================================================

// FIX: Usamos a inicialização manual para evitar ambiguidades com o AddAutoMapper.
// Isso garante que a configuração e o registro do IMapper sejam feitos explicitamente.
var mapperConfig = new MapperConfiguration(cfg =>
{
    // Usa o método AddProfile, passando o tipo, para carregar os perfis.
    cfg.AddProfile<ArtigoMappingProfile>();
    cfg.AddProfile<PersistenceMappingProfile>();
});

// Valida a configuração (opcional, mas recomendado)
mapperConfig.AssertConfigurationIsValid();

// Registra a instância IMapper como Singleton.
builder.Services.AddSingleton<IMapper>(sp => mapperConfig.CreateMapper());


// =========================================================================
// 3. REGISTRO DE REPOSITORIES E SERVICES (DI)
// (Unchanged from last version)
// =========================================================================

builder.Services.AddScoped<IArtigoRepository, ArtigoRepository>();
builder.Services.AddScoped<IAutorRepository, AutorRepository>();
builder.Services.AddScoped<IEditorialRepository, EditorialRepository>();
builder.Services.AddScoped<IArtigoHistoryRepository, ArtigoHistoryRepository>();
builder.Services.AddScoped<IInteractionRepository, InteractionRepository>();
builder.Services.AddScoped<IPendingRepository, PendingRepository>();
builder.Services.AddScoped<IStaffRepository, StaffRepository>();
builder.Services.AddScoped<IVolumeRepository, VolumeRepository>();

builder.Services.AddScoped<IArtigoService, ArtigoService>();


// =========================================================================
// 4. CONFIGURAÇÃO DO HOT CHOCOLATE (GraphQL)
// =========================================================================

var graphQLServer = builder.Services.AddGraphQLServer()
    .AddAuthorization()
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

    // FIX 1: Adiciona o tipo ArtigoHistory (criado no passo anterior)
    .AddType<ArtigoHistoryType>()
    .AddType<EditorialTeamType>()
    .AddType<ContribuicaoEditorialType>()

    // Mapeia Enums
    .BindRuntimeType<ArtigoStatus, EnumType<ArtigoStatus>>()
    .BindRuntimeType<ArtigoTipo, EnumType<ArtigoTipo>>()
    .BindRuntimeType<EditorialPosition, EnumType<EditorialPosition>>()
    .BindRuntimeType<ContribuicaoRole, EnumType<ContribuicaoRole>>() // Adicionado para completude
    .BindRuntimeType<Artigo.Intf.Enums.InteractionType, EnumType<Artigo.Intf.Enums.InteractionType>>()
    .BindRuntimeType<PendingStatus, EnumType<PendingStatus>>()
    .BindRuntimeType<TargetEntityType, EnumType<TargetEntityType>>()
    .BindRuntimeType<JobRole, EnumType<JobRole>>()
    .BindRuntimeType<VolumeMes, EnumType<VolumeMes>>()

    // Configura DataLoaders
    .AddDataLoader<EditorialDataLoader>()
    .AddDataLoader<VolumeDataLoader>()
    .AddDataLoader(sp =>
        new ArtigoGroupedDataLoader(
            sp.GetRequiredService<IBatchScheduler>(),
            sp.GetRequiredService<IArtigoRepository>(),
            sp.GetRequiredService<IMapper>()
        )
    )
    .AddDataLoader<AutorDataLoader>()
    .AddDataLoader<InteractionDataLoader>()
    .AddDataLoader<CurrentHistoryContentDataLoader>()
    .AddDataLoader<ArtigoHistoryGroupedDataLoader>()
    .AddDataLoader<InteractionRepliesDataLoader>()

    // Configuração para uso com MongoDB
    .AddMongoDbProjections()
    .AddMongoDbFiltering()
    .AddMongoDbSorting();

// =========================================================================
// 5. CONFIGURAÇÃO DE AUTENTICAÇÃO E ROTEAMENTO
// (Unchanged from last version)
// =========================================================================

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
// (Unchanged from last version)
// =========================================================================

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapGraphQL();
});


app.Run();


// =========================================================================
// Placeholder Definitions and Injectable DataLoaders (Final)
// =========================================================================

// DataLoaders/Context defined previously and injected:
public class ArtigoGroupedDataLoader : GroupedDataLoader<string, Artigo.Server.DTOs.ArtigoDTO>
{
    private readonly IArtigoRepository _artigoRepository;
    private readonly IMapper _mapper;

    public ArtigoGroupedDataLoader(
        IBatchScheduler scheduler,
        IArtigoRepository artigoRepository,
        IMapper mapper)
        : base(scheduler)
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
