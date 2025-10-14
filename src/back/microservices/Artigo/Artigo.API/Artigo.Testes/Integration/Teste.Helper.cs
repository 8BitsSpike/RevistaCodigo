using Artigo.DbContext.Data;
using Artigo.DbContext.Mappers;
using Artigo.DbContext.Repositories;
using Artigo.Intf.Interfaces;
using Artigo.Server.Mappers;
using Artigo.Server.Services;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Xunit;

namespace Artigo.Testes.Integration
{
    // Usado para garantir que a conexão e o banco de dados sejam configurados uma vez por classe de teste.
    public class ArtigoIntegrationTestFixture
    {
        public IServiceProvider ServiceProvider { get; }
        private const string TestDatabaseName = "MagazineArtigoTestDB";
        private const string MongoConnectionString = "mongodb://localhost:27017";

        public ArtigoIntegrationTestFixture()
        {
            var services = new ServiceCollection();

            // 1. Configuração do AutoMapper - FIX: Usando a inicialização explícita
            services.AddSingleton<AutoMapper.IMapper>(sp =>
            {
                // Inject ILoggerFactory to satisfy the MapperConfiguration constructor's strict requirement.
                var loggerFactory = sp.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>();

                var mapperConfig = new AutoMapper.MapperConfiguration(cfg =>
                {
                    cfg.AddProfile<ArtigoMappingProfile>();
                    cfg.AddProfile<PersistenceMappingProfile>();
                }, loggerFactory); // Explicitly pass the logger factory

                mapperConfig.AssertConfigurationIsValid();
                return mapperConfig.CreateMapper();
            });

            // Ensure ILoggerFactory is available in the services collection
            services.AddLogging(); // Provides ILoggerFactory dependency

            // 2. Configuração do MongoDB e Contexto (remains the same)
            var mongoClient = new MongoClient(MongoConnectionString);
            // ... (rest of the mongo setup) ...

            // 3. Registro de Repositories e Services (remains the same)
            // ...

            ServiceProvider = services.BuildServiceProvider();
        }

        // Método para limpar o banco de dados de teste após a execução dos testes
        public void Dispose()
        {
            // Opcional: Implementar a remoção do banco de dados de teste aqui
            // var client = new MongoClient(MongoConnectionString);
            // client.DropDatabase(TestDatabaseName);
        }
    }
}