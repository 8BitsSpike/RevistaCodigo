using Artigo.API.GraphQL.DataLoaders;
using Artigo.API.GraphQL.Mutations;
using Artigo.API.GraphQL.Queries;
using Artigo.API.GraphQL.Types;
using Artigo.DbContext.Data;
using Artigo.DbContext.Mappers;
using Artigo.DbContext.Repositories;
using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using Artigo.Intf.Interfaces;
using Artigo.Server.Mappers;
using Artigo.Server.Services;
using HotChocolate.Data.MongoDb;
using HotChocolate.Execution; // Added for IErrorFilter
using HotChocolate.Types;
using Microsoft.Extensions.Logging; // Added for ILoggerFactory (AutoMapper Fix)
using MongoDB.Driver;
using System.Reflection; // Added for reflection (AutoMapper Fix)


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
builder.Services.AddSingleton<Artigo.DbContext.Interfaces.IMongoDbContext>(sp =>
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

// FIX: Usamos a inicialização explícita e registro para evitar ambiguidades.
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
    .AddQueryType<ArtigoQueries>()
    .AddMutationType<ArtigoMutations>()

    // FIX: Add Error Filters for graceful exception handling
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

    // Configura DataLoaders
    .AddDataLoader<EditorialDataLoader>()
    .AddDataLoader<VolumeDataLoader>()
    .AddDataLoader<AutorDataLoader>()
    .AddDataLoader<InteractionDataLoader>()
    .AddDataLoader<CurrentHistoryContentDataLoader>()
    .AddDataLoader<ArtigoHistoryGroupedDataLoader>()
    .AddDataLoader<InteractionRepliesDataLoader>()
    .AddDataLoader<ExternalUserDataLoader>()
    .AddDataLoader(sp =>
        new Artigo.API.GraphQL.DataLoaders.ArtigoGroupedDataLoader(
            sp.GetRequiredService<GreenDonut.IBatchScheduler>(),
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

app.MapGraphQL();


app.Run();


// =========================================================================
// Helper Definitions (Needed for compilation: Error Handlers & Resolvers)
// =========================================================================

// --- Error Filter Implementations ---

public class AuthorizationErrorFilter : IErrorFilter
{
    public IError OnError(IError error)
    {
        if (error.Exception is UnauthorizedAccessException)
        {
            return error.WithCode("AUTH_FORBIDDEN").WithMessage("Acesso negado. O usuário não tem as permissões necessárias para executar esta ação.");
        }
        return error;
    }
}

public class ApplicationErrorFilter : IErrorFilter
{
    public IError OnError(IError error)
    {
        switch (error.Exception)
        {
            case InvalidOperationException ioe:
                return error.WithCode("BUSINESS_INVALID_OPERATION").WithMessage(ioe.Message);
            case System.Collections.Generic.KeyNotFoundException knfe:
                return error.WithCode("RESOURCE_NOT_FOUND").WithMessage(knfe.Message);
            default:
                // FIX: Removed unsupported methods (RemoveException/RemoveExtensions) 
                // and sanitized the message directly.
                return error.WithMessage("Ocorreu um erro interno de processamento.");
        }
    }
}


// --- Resolver Implementations (Placeholders for Type references in *Type.cs) ---

public class EditorialResolver
{
    public Task<Artigo.Intf.Entities.Editorial?> GetEditorialAsync(Artigo.Server.DTOs.ArtigoDTO artigo, Artigo.API.GraphQL.DataLoaders.EditorialDataLoader dataLoader, HotChocolate.Resolvers.IResolverContext context) => throw new NotImplementedException();
}
public class AutorResolver
{
    public Task<IReadOnlyList<Artigo.Intf.Entities.Autor>> GetAutoresAsync(Artigo.Server.DTOs.ArtigoDTO artigo, Artigo.API.GraphQL.DataLoaders.AutorDataLoader dataLoader, HotChocolate.Resolvers.IResolverContext context, CancellationToken cancellationToken) => throw new NotImplementedException();
}
public class ArtigoHistoryResolver
{
    public Task<string> GetCurrentContentAsync(Artigo.Server.DTOs.ArtigoDTO artigo, Artigo.API.GraphQL.DataLoaders.CurrentHistoryContentDataLoader dataLoader, HotChocolate.Resolvers.IResolverContext context) => throw new NotImplementedException();
}
public class VolumeResolver
{
    public Task<Artigo.Intf.Entities.Volume?> GetVolumeAsync(Artigo.Server.DTOs.ArtigoDTO artigo, Artigo.API.GraphQL.DataLoaders.VolumeDataLoader dataLoader, HotChocolate.Resolvers.IResolverContext context) => throw new NotImplementedException();
}
public class ArtigoHistoryListResolver
{
    public Task<IReadOnlyList<Artigo.Intf.Entities.ArtigoHistory>> GetHistoryAsync(Artigo.Intf.Entities.Editorial editorial, Artigo.API.GraphQL.DataLoaders.ArtigoHistoryGroupedDataLoader dataLoader, CancellationToken cancellationToken) => throw new NotImplementedException();
}
public class InteractionListResolver
{
    public Task<IReadOnlyList<Artigo.Intf.Entities.Interaction>> GetEditorialCommentsAsync(Artigo.Intf.Entities.Editorial editorial, Artigo.API.GraphQL.DataLoaders.InteractionDataLoader dataLoader, CancellationToken cancellationToken) => throw new NotImplementedException();
}
public class RepliesResolver
{
    public Task<IReadOnlyList<Artigo.Intf.Entities.Interaction>> GetRepliesAsync(Artigo.Intf.Entities.Interaction parentComment, Artigo.API.GraphQL.DataLoaders.InteractionRepliesDataLoader dataLoader, CancellationToken cancellationToken) => throw new NotImplementedException();
}
public class ArticleInVolumeResolver
{
    public Task<IReadOnlyList<Artigo.Server.DTOs.ArtigoDTO>> GetArticlesAsync(Artigo.Intf.Entities.Volume volume, Artigo.API.GraphQL.DataLoaders.ArtigoGroupedDataLoader dataLoader, CancellationToken cancellationToken) => throw new NotImplementedException();
}
// Placeholder for EditorialTeamType 
public class EditorialTeamType : HotChocolate.Types.ObjectType<Artigo.Intf.Entities.EditorialTeam>
{
    protected override void Configure(HotChocolate.Types.IObjectTypeDescriptor<Artigo.Intf.Entities.EditorialTeam> descriptor)
    {
        descriptor.Field(f => f.EditorId);
    }
}
// Placeholder for ContribuicaoEditorialType 
public class ContribuicaoEditorialType : HotChocolate.Types.ObjectType<Artigo.Intf.Entities.ContribuicaoEditorial>
{
    protected override void Configure(HotChocolate.Types.IObjectTypeDescriptor<Artigo.Intf.Entities.ContribuicaoEditorial> descriptor)
    {
        descriptor.Field(f => f.ArtigoId);
    }
}

// DataLoaders are defined in their own files but ArtigoGroupedDataLoader's definition 
// remains here as it's manually instantiated in the DI setup (Section 4).
public class ArtigoGroupedDataLoader : GreenDonut.GroupedDataLoader<string, Artigo.Server.DTOs.ArtigoDTO>
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

    protected override async Task<System.Linq.ILookup<string, Artigo.Server.DTOs.ArtigoDTO>> LoadGroupedBatchAsync(System.Collections.Generic.IReadOnlyList<string> keys, System.Threading.CancellationToken cancellationToken)
    {
        var artigos = await _artigoRepository.GetByIdsAsync(keys.ToList());
        var dtos = _mapper.Map<System.Collections.Generic.IReadOnlyList<Artigo.Server.DTOs.ArtigoDTO>>(artigos);
        return dtos.ToLookup(a => a.Id, a => a);
    }
}