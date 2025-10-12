using Artigo.API.GraphQL.DataLoaders;
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
using Artigo.Server.DTOs;
using Artigo.Server.Mappers;
using Artigo.Server.Services;
using AutoMapper;
using HotChocolate.Data;
using HotChocolate.Execution.Configuration;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// =========================================================================
// 1. CONFIGURAÇÃO DO MONGODB (Database Settings)
// =========================================================================

// Configuração de opções a partir do appsettings.json (Exemplo: "MongoDbSettings:ConnectionString")
// builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));

// A. Singleton do IMongoClient e do Contexto (Conexão Centralizada)
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    // Usar uma string de conexão real do Configuration
    var connectionString = builder.Configuration.GetConnectionString("MongoDb")
        ?? "mongodb://localhost:27017"; // Fallback local
    return new MongoClient(connectionString);
});

// B. Implementação do contexto de dados
builder.Services.AddSingleton<IMongoDbContext>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    // Usar o nome do banco de dados do Configuration
    var databaseName = builder.Configuration["MongoDb:DatabaseName"] ?? "MagazineArtigoDB";
    return new MongoDbContext(client, databaseName);
});

// C. Registra a classe de Contexto (não o contrato) para injeção
builder.Services.AddSingleton<MongoDbContext>();


// =========================================================================
// 2. CONFIGURAÇÃO DO AUTOMAPPER
// =========================================================================

// Cria uma instância do IConfigurationProvider, incluindo todos os perfis.
var mapperConfig = new MapperConfiguration(cfg =>
{
    // Perfil da Camada Application -> DTO
    cfg.AddProfile<ArtigoMappingProfile>();
    // Perfil da Camada Persistence -> Domain
    cfg.AddProfile<PersistenceMappingProfile>();
});

// Registra o IMapper singleton
builder.Services.AddSingleton(mapperConfig.CreateMapper());


// =========================================================================
// 3. REGISTRO DE REPOSITORIES E SERVICES (DI)
// =========================================================================

// Repositories (Infrastructure/DbContext Layer)
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
    .AddAuthorization() // Adiciona suporte a [Authorize]
    .AddFiltering()     // Adiciona suporte a [UseFiltering]
    .AddSorting()       // Adiciona suporte a [UseSorting]
    .AddProjections()   // Adiciona suporte a otimização de queries de projeção
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
    .AddType<ArtigoHistoryType>() // Necessário adicionar o ArtigoHistoryType
    .AddType<EditorialTeamType>() // Necessário adicionar tipos embutidos
    .AddType<ContribuicaoEditorialType>()
    // Mapeia Enums
    .BindRuntimeType<ArtigoStatus, EnumType<ArtigoStatus>>()
    .BindRuntimeType<ArtigoTipo, EnumType<ArtigoTipo>>()
    .BindRuntimeType<EditorialPosition, EnumType<EditorialPosition>>()
    // ... (Outros Enums)

    // Configura DataLoaders (Injetando dependências de Repositório/Mapper)
    .AddDataLoader<EditorialDataLoader>()
    .AddDataLoader<VolumeDataLoader>()
    // O ArtigoGroupedDataLoader precisa ser reescrito para injetar IArtigoRepository e IMapper
    // Criaremos uma implementação que lida com isso.
    .AddDataLoader(sp =>
        new ArtigoGroupedDataLoader(
            sp.GetRequiredService<IBatchScheduler>(),
            sp.GetRequiredService<IArtigoRepository>(),
            sp.GetRequiredService<IMapper>()
        )
    )
    // Demais DataLoaders...
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
// =========================================================================

// Configuração da Autenticação (JWT Bearer Token)
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        // Esta é uma simulação; use as configurações reais do UsuarioAPI
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
app.UseAuthorization(); // Deve vir depois de UseRouting/UseAuthentication

// Endpoint principal para o GraphQL
app.UseEndpoints(endpoints =>
{
    endpoints.MapGraphQL();

    // Opcional: Adiciona o Banana Cake Pop (GraphQL IDE) em /graphql-ui
    // endpoints.MapBananaCakePop("/graphql-ui"); 
});


app.Run();


// =========================================================================
// Implementação do ArtigoGroupedDataLoader com dependências injetadas
// =========================================================================
public class ArtigoGroupedDataLoader : GroupedDataLoader<string, ArtigoDTO>
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

    protected override async Task<ILookup<string, ArtigoDTO>> LoadGroupedBatchAsync(IReadOnlyList<string> keys, CancellationToken cancellationToken)
    {
        // 1. Busca as Entidades do Domínio em lote
        var artigos = await _artigoRepository.GetByIdsAsync(keys.ToList());

        // 2. Mapeia as Entidades (Artigo) para DTOs (ArtigoDTO)
        var dtos = _mapper.Map<IReadOnlyList<ArtigoDTO>>(artigos);

        // 3. Retorna como ILookup, usando o ID do Artigo como chave de agrupamento
        return dtos.ToLookup(a => a.Id, a => a);
    }
}

// Necessário para compilação, pois esses tipos foram definidos no ArtigoType.cs/EditorialType.cs
// Idealmente estariam em arquivos separados.
public class ArtigoHistoryType : ObjectType<ArtigoHistory> { protected override void Configure(IObjectTypeDescriptor<ArtigoHistory> descriptor) { descriptor.Field(f => f.Id); descriptor.Field(f => f.Content); descriptor.Field(f => f.Version); } }
public class EditorialTeamType : ObjectType<EditorialTeam> { protected override void Configure(IObjectTypeDescriptor<EditorialTeam> descriptor) { descriptor.Field(f => f.EditorId); descriptor.Field(f => f.ReviewerIds); } }
public class ContribuicaoEditorialType : ObjectType<ContribuicaoEditorial> { protected override void Configure(IObjectTypeDescriptor<ContribuicaoEditorial> descriptor) { descriptor.Field(f => f.ArtigoId); descriptor.Field(f => f.Role); } }