using Artigo.API.GraphQL.DataLoaders; // Necessário para referenciar DataLoaders
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
// 1. CONFIGURAÇÃO DO MONGODB
// =========================================================================

// A. Singleton do IMongoClient (Conexão Centralizada)
//builder.Services.AddSingleton<IMongoClient>(sp =>
//{
//    var connectionString = builder.Configuration.GetConnectionString("MongoDb")
//        ?? "mongodb://localhost:27017/";
//    return new MongoClient(connectionString);
//});

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
// 4. CONFIGURAÇÃO DO HOT CHOCOLATE (GraphQL)
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
    .BindRuntimeType<ArtigoStatus, EnumType<ArtigoStatus>>()
    .BindRuntimeType<ArtigoTipo, EnumType<ArtigoTipo>>()
    .BindRuntimeType<EditorialPosition, EnumType<EditorialPosition>>()
    .BindRuntimeType<ContribuicaoRole, EnumType<ContribuicaoRole>>()
    .BindRuntimeType<Artigo.Intf.Enums.InteractionType, EnumType<Artigo.Intf.Enums.InteractionType>>()
    .BindRuntimeType<PendingStatus, EnumType<PendingStatus>>()
    .BindRuntimeType<TargetEntityType, EnumType<TargetEntityType>>()
    .BindRuntimeType<JobRole, EnumType<JobRole>>()
    .BindRuntimeType<VolumeMes, EnumType<VolumeMes>>()

    // Configura DataLoaders (registro simplificado)
    .AddDataLoader<EditorialDataLoader>()
    .AddDataLoader<VolumeDataLoader>()
    .AddDataLoader<AutorDataLoader>()
    .AddDataLoader<InteractionDataLoader>()
    .AddDataLoader<CurrentHistoryContentDataLoader>()
    .AddDataLoader<ArtigoHistoryGroupedDataLoader>()
    .AddDataLoader<InteractionRepliesDataLoader>()
    .AddDataLoader<ExternalUserDataLoader>()
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
// 6. MIDDLEWARE PIPELINE (Modernized Routing)
// =========================================================================

app.UseAuthentication();
app.UseAuthorization();

// Top-level route registration
app.MapGraphQL();

app.Run();
