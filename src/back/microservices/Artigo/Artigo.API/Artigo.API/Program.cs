using Artigo.API.GraphQL.DataLoaders; // Necess�rio para referenciar DataLoaders
using Artigo.API.GraphQL.ErrorFilters; // Adicionado para referenciar os novos filtros de erro
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

var builder = WebApplication.CreateBuilder(args);

// =========================================================================
// 1. CONFIGURA��O DO MONGODB
// =========================================================================

// A. Singleton do IMongoClient (Conex�o Centralizada)
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("MongoDb")
        ?? "mongodb://localhost:27017/";
    return new MongoClient(connectionString);
});

// B. Implementa��o do contexto de dados (IMongoDbContext)
builder.Services.AddSingleton<Artigo.DbContext.Interfaces.IMongoDbContext>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var databaseName = builder.Configuration["MongoDb:DatabaseName"] ?? "RBEB";
    return new MongoDbContext(client, databaseName);
});


// =========================================================================
// 2. CONFIGURA��O DO AUTOMAPPER
// =========================================================================

// Usando o m�todo de extens�o AddAutoMapper para uma configura��o mais limpa
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
builder.Services.AddScoped<Artigo.Server.Interfaces.IExternalUserService, Artigo.Server.Services.ExternalUserService>();


// =========================================================================
// 4. CONFIGURA��O DO HOT CHOCOLATE (GraphQL)
// =========================================================================

var graphQLServer = builder.Services.AddGraphQLServer()
    .AddAuthorizationCore()
    .AddFiltering()
    .AddSorting()
    .AddProjections()

    // --- FIX: Usa os wrappers definidos manualmente ---
    .AddQueryType<ArtigoQueryType>()
    .AddMutationType<ArtigoMutationType>()
    // --- FIM FIX ---

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
    .AddType<ExternalUserType>()

    // Mapeia Enums
    .BindRuntimeType<StatusArtigo, EnumType<StatusArtigo>>() // FIX: ArtigoStatus -> StatusArtigo
    .BindRuntimeType<TipoArtigo, EnumType<TipoArtigo>>() // FIX: ArtigoTipo -> TipoArtigo
    .BindRuntimeType<PosicaoEditorial, EnumType<PosicaoEditorial>>() // FIX: EditorialPosition -> PosicaoEditorial
    .BindRuntimeType<FuncaoContribuicao, EnumType<FuncaoContribuicao>>() // FIX: ContribuicaoRole -> FuncaoContribuicao
    .BindRuntimeType<TipoInteracao, EnumType<TipoInteracao>>() // FIX: InteractionType -> TipoInteracao
    .BindRuntimeType<StatusPendente, EnumType<StatusPendente>>() // FIX: PendingStatus -> StatusPendente
    .BindRuntimeType<TipoEntidadeAlvo, EnumType<TipoEntidadeAlvo>>() // FIX: TargetEntityType -> TipoEntidadeAlvo
    .BindRuntimeType<FuncaoTrabalho, EnumType<FuncaoTrabalho>>() // FIX: JobRole -> FuncaoTrabalho
    .BindRuntimeType<MesVolume, EnumType<MesVolume>>() // FIX: VolumeMes -> MesVolume

    // Configura DataLoaders (registro simplificado)
    .AddDataLoader<EditorialDataLoader>()
    .AddDataLoader<VolumeDataLoader>()
    .AddDataLoader<AutorBatchDataLoader>()
    .AddDataLoader<InteractionDataLoader>()
    .AddDataLoader<CurrentHistoryContentDataLoader>()
    .AddDataLoader<ArtigoHistoryGroupedDataLoader>()
    .AddDataLoader<InteractionRepliesDataLoader>()
    .AddDataLoader<ExternalUserDataLoader>()
    .AddDataLoader<Artigo.API.GraphQL.DataLoaders.ArtigoGroupedDataLoader>()

    // Configura��o para uso com MongoDB
    .AddMongoDbProjections()
    .AddMongoDbFiltering()
    .AddMongoDbSorting();


// =========================================================================
// 5. CONFIGURA��O DE AUTENTICA��O E ROTEAMENTO
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
// 6. MIDDLEWARE PIPELINE (Modernized Routing)
// =========================================================================

app.UseAuthentication();
app.UseAuthorization();

// Top-level route registration
app.MapGraphQL();

app.Run();