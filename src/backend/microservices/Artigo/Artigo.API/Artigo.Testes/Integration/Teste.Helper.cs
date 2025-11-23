using Artigo.DbContext.Data;
using Artigo.DbContext.Mappers;
using Artigo.DbContext.Repositories;
using Artigo.Intf.Interfaces;
using Artigo.Server.Mappers;
using Artigo.Server.Services;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Xunit;
using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using System;
using System.Threading.Tasks;

namespace Artigo.Testes.Integration
{
    // Usado para garantir que a conexão e o banco de dados sejam configurados uma vez por classe de teste.
    public class ArtigoIntegrationTestFixture : IDisposable
    {
        public IServiceProvider ServiceProvider { get; }
        private const string TestDatabaseName = "RBEB_TEST";
        private const string MongoConnectionString = "mongodb://localhost:27017";

        // ID de usuário Administrador de teste (para checagem de autorização)
        private const string AdminTestUsuarioId = "test_admin_401";

        public ArtigoIntegrationTestFixture()
        {
            var services = new ServiceCollection();

            services.AddLogging();

            // 1. Configuração do AutoMapper
            services.AddSingleton<AutoMapper.IMapper>(sp =>
            {
                var mapperConfig = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile<PersistenceMappingProfile>();
                    cfg.AddProfile<ArtigoMappingProfile>();
                });

                return mapperConfig.CreateMapper();
            });

            // 2. Configuração do MongoDB
            services.AddSingleton<IMongoClient>(sp =>
            {
                return new MongoClient(MongoConnectionString);
            });

            services.AddSingleton<Artigo.DbContext.Interfaces.IMongoDbContext>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                return new MongoDbContext(client, TestDatabaseName);
            });

            // 3. Repositórios e UnitOfWork
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IArtigoRepository, ArtigoRepository>();
            services.AddScoped<IEditorialRepository, EditorialRepository>();
            services.AddScoped<IArtigoHistoryRepository, ArtigoHistoryRepository>();
            services.AddScoped<IAutorRepository, AutorRepository>();
            services.AddScoped<IStaffRepository, StaffRepository>();
            services.AddScoped<IVolumeRepository, VolumeRepository>();
            services.AddScoped<IPendingRepository, PendingRepository>();
            services.AddScoped<IInteractionRepository, InteractionRepository>();

            // 4. Serviço Principal
            services.AddScoped<IArtigoService, ArtigoService>();

            // Constrói o provedor
            ServiceProvider = services.BuildServiceProvider();

            // Inicializa dados básicos (como o Admin Staff)
            SetupInitialStaff(ServiceProvider).GetAwaiter().GetResult();
        }

        private async Task SetupInitialStaff(IServiceProvider serviceProvider)
        {
            // O IStaffRepository é Scoped, então precisamos de um novo escopo para resolvê-lo.
            using var scope = serviceProvider.CreateScope();
            var staffRepository = scope.ServiceProvider.GetRequiredService<IStaffRepository>();

            var existingStaff = await staffRepository.GetByUsuarioIdAsync(AdminTestUsuarioId);

            if (existingStaff == null)
            {
                var adminStaff = new Staff
                {
                    // ID vazio para o Mongo gerar
                    Id = string.Empty,
                    UsuarioId = AdminTestUsuarioId,
                    Job = FuncaoTrabalho.Administrador,
                    IsActive = true,
                    Nome = "Admin Teste",
                    Url = "http://avatar.com/admin.jpg"
                };

                await staffRepository.AddAsync(adminStaff);
            }
        }

        /// <sumario>
        /// Método para LIMPAR (DELETAR) o banco de dados de teste após a execução dos testes.
        /// CORRIGIDO: Agora deleta o banco de dados de teste 'RBEB_TEST'.
        /// </sumario>
        public void Dispose()
        {
            var client = new MongoClient(MongoConnectionString);
            // Deleta o banco de testes após o uso para limpeza total
            client.DropDatabase(TestDatabaseName);
        }
    }
}