using Artigo.API.GraphQL.DataLoaders;
using Artigo.API.GraphQL.ErrorFilters;
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
using HotChocolate.Data.MongoDb;
using MongoDB.Driver;
using Artigo.API.GraphQL.Inputs;

var builder = WebApplication.CreateBuilder(args);

// =========================================================================
// 1. CONFIGURAÇÃO DO MONGODB
// =========================================================================

// A. Singleton do IMongoClient (Conexão Centralizada)
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("MongoDb")
        ?? "mongodb://localhost:27017/";
    return new MongoClient(connectionString);
});

// B. Implementação do contexto de dados (IMongoDbContext)
builder.Services.AddSingleton<Artigo.DbContext.Interfaces.IMongoDbContext>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var databaseName = builder.Configuration["MongoDb:DatabaseName"] ?? "RBEB";
    return new MongoDbContext(client, databaseName);
});


// =========================================================================
// 2. CONFIGURAÇÃO DO AUTOMAPPER
// =========================================================================

// Usando o método de extensão AddAutoMapper para uma configuração mais limpa
builder.Services.AddSingleton<AutoMapper.IMapper>(sp =>
{
    var loggerFactory = sp.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>();
    var mapperConfig = new AutoMapper.MapperConfiguration(cfg =>
    {
        cfg.AddProfile<ArtigoMappingProfile>();
        cfg.AddProfile<PersistenceMappingProfile>();
    }, loggerFactory);
    mapperConfig.AssertConfigurationIsValid();
    return mapperConfig.CreateMapper();
});


// =========================================================================
// 3. REGISTRO DE REPOSITORIES E SERVICES (DI)
// =========================================================================

// *** NOVO (Unit of Work) ***
// Scoped: uma nova instância por requisição HTTP (GraphQL)
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Repositories (Scoped Lifetime para maior segurança)
builder.Services.AddScoped<IArtigoRepository, ArtigoRepository>();
builder.Services.AddScoped<IAutorRepository, AutorRepository>();
builder.Services.AddScoped<IEditorialRepository, EditorialRepository>();
builder.Services.AddScoped<IArtigoHistoryRepository, ArtigoHistoryRepository>();
builder.Services.AddScoped<IInteractionRepository, InteractionRepository>();
builder.Services.AddScoped<IPendingRepository, PendingRepository>();
builder.Services.AddScoped<IStaffRepository, StaffRepository>();
builder.Services.AddScoped<IVolumeRepository, VolumeRepository>();

// Services (Camada de aplicação dos serviços)
builder.Services.AddScoped<IArtigoService, ArtigoService>();
// Removido: IExternalUserService


// =========================================================================
// 4. CONFIGURAÇÃO DO HOT CHOCOLATE (GraphQL)
// =========================================================================

var graphQLServer = builder.Services.AddGraphQLServer()
    .AddAuthorizationCore()
    .AddFiltering()
    .AddSorting()
    .AddProjections()

    // --- wrapper definido manualmente ---
    .AddQueryType<ArtigoQueryType>()
    .AddMutationType<ArtigoMutationType>()

    // Registra os filtros de erro personalizados
    .AddErrorFilter<AuthorizationErrorFilter>()
    .AddErrorFilter<ApplicationErrorFilter>()

    // Mapeia todos os tipos definidos
    .AddType<ArtigoType>()
    .AddType<EditorialType>()
    .AddType<AutorType>()
    .AddType<VolumeType>()
    .AddType<Artigo.API.GraphQL.Types.InteractionType>()
    .AddType<PendingType>()
    .AddType<StaffType>()
    .AddType<ArtigoHistoryType>()
    // Removido: .AddType<ExternalUserType>()

    // *** NOVOS TIPOS (Formatos e StaffComentario) ***
    .AddType<ArtigoCardListType>()
    // Removido: .AddType<VolumeListType>() // Obsoleto
    .AddType<AutorViewType>()
    .AddType<ArtigoViewType>()
    .AddType<StaffComentarioType>()

    // *** TIPOS ADICIONADOS (PRIORIDADE 1 e 2) ***
    .AddType<VolumeCardType>()
    .AddType<ArtigoEditorialViewType>()
    .AddType<EditorialViewType>()
    .AddType<ArtigoHistoryEditorialViewType>()
    .AddType<InteractionConnectionDTOType>()
    .AddType<ArtigoHistoryViewType>()
    .AddType<StaffViewDTOType>() // Adicionado na Prioridade 4
    .AddType<VolumeViewType>() // Adicionado na Prioridade 2


    // *** NOVOS TIPOS DE INPUT ***
    .AddType<AutorInputType>()
    .AddType<CreateStaffInput>()
    .AddType<MidiaEntryInputType>() // Adicionado na Prioridade 2
    .AddType<UpdateVolumeMetadataInputType>() // Adicionado na Prioridade 5
    .AddType<MidiaEntryEntityInputType>() // Adicionado na Prioridade 5
    .AddType<CreateVolumeInputType>() // *** ADICIONADO (CORREÇÃO) ***
    .AddType<EditorialTeamInputType>() // *** ADICIONADO (Plano B) ***

    // Mapeia Enums
    .BindRuntimeType<StatusArtigo, EnumType<StatusArtigo>>()
    .BindRuntimeType<TipoArtigo, EnumType<TipoArtigo>>()
    .BindRuntimeType<PosicaoEditorial, EnumType<PosicaoEditorial>>()
    .BindRuntimeType<FuncaoContribuicao, EnumType<FuncaoContribuicao>>()
    .BindRuntimeType<TipoInteracao, EnumType<TipoInteracao>>()
    .BindRuntimeType<StatusPendente, EnumType<StatusPendente>>()
    .BindRuntimeType<TipoEntidadeAlvo, EnumType<TipoEntidadeAlvo>>()
    .BindRuntimeType<FuncaoTrabalho, EnumType<FuncaoTrabalho>>()
    .BindRuntimeType<MesVolume, EnumType<MesVolume>>()
    .BindRuntimeType<StatusVolume, EnumType<StatusVolume>>() // Adicionado na Prioridade 1

    // Configura DataLoaders (registro simplificado)
    .AddDataLoader<EditorialDataLoader>()
    .AddDataLoader<VolumeDataLoader>()
    .AddDataLoader<AutorBatchDataLoader>()
    .AddDataLoader<InteractionDataLoader>()
    .AddDataLoader<CurrentHistoryContentDataLoader>()
    .AddDataLoader<ArtigoHistoryGroupedDataLoader>()
    .AddDataLoader<InteractionRepliesDataLoader>()
    // Removido: ExternalUserDataLoader
    .AddDataLoader<Artigo.API.GraphQL.DataLoaders.ArticleInteractionsDataLoader>()
    .AddDataLoader<Artigo.API.GraphQL.DataLoaders.ArtigoGroupedDataLoader>()

    // Configuração para uso com MongoDB
    .AddMongoDbProjections()
    .AddMongoDbFiltering()
    .AddMongoDbSorting();


// =========================================================================
// 5. CONFIGURAÇÃO DE AUTENTICAÇÃO E ROTEAMENTO
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
// =========================================================================

app.UseAuthentication();
app.UseAuthorization();

app.MapGraphQL();

app.Run();