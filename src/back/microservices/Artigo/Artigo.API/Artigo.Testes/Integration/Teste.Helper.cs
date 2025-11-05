using Artigo.DbContext.Data;
using Artigo.DbContext.Mappers;
using Artigo.DbContext.Repositories;
using Artigo.Intf.Interfaces;
using Artigo.Server.Mappers;
using Artigo.Server.Services;
// Removido: using Artigo.Server.Interfaces;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Xunit;
using Microsoft.Extensions.Logging;
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
        private const string TestDatabaseName = "RBEB";
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
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

                var mapperConfig = new AutoMapper.MapperConfiguration(cfg =>
                {
                    cfg.AddProfile<ArtigoMappingProfile>();
                    cfg.AddProfile<PersistenceMappingProfile>();
                }, loggerFactory);

                mapperConfig.AssertConfigurationIsValid();
                return mapperConfig.CreateMapper();
            });

            // 2. Configuração do MongoDB e Contexto
            var mongoClient = new MongoClient(MongoConnectionString);

            services.AddSingleton<IMongoClient>(mongoClient);

            services.AddSingleton<Artigo.DbContext.Interfaces.IMongoDbContext>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                return new MongoDbContext(client, TestDatabaseName);
            });

            // 3. REGISTRO DE REPOSITORIES E SERVICES

            // *** NOVO (Unit of Work) ***
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IArtigoRepository, ArtigoRepository>();
            services.AddScoped<IAutorRepository, AutorRepository>();
            services.AddScoped<IEditorialRepository, EditorialRepository>();
            services.AddScoped<IArtigoHistoryRepository, ArtigoHistoryRepository>();
            services.AddScoped<IInteractionRepository, InteractionRepository>();
            services.AddScoped<IPendingRepository, PendingRepository>();
            services.AddScoped<IStaffRepository, StaffRepository>();
            services.AddScoped<IVolumeRepository, VolumeRepository>();

            services.AddScoped<IArtigoService, ArtigoService>();
            // Removido: IExternalUserService


            ServiceProvider = services.BuildServiceProvider();

            // 4. SETUP INICIAL DO BANCO DE DADOS (INSERÇÃO DE STAFF ADMINISTRADOR)
            SetupInitialStaff(ServiceProvider).GetAwaiter().GetResult();
        }

        /// <sumario>
        /// Garante que o Staff Administrador necessário para testes de autorização exista.
        /// </sumario>
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
                    Id = string.Empty,
                    UsuarioId = AdminTestUsuarioId,
                    Job = FuncaoTrabalho.Administrador,
                    IsActive = true,
                    // *** NOVOS CAMPOS ***
                    Nome = "Admin Teste",
                    Url = "http://avatar.com/admin.jpg"
                };

                // Nota: Usando AddAsync do IStaffRepository para persistir.
                await staffRepository.AddAsync(adminStaff);
            }
        }

        /// <sumario>
        /// Método para LIMPAR (DELETAR) o banco de dados de teste após a execução dos testes.
        /// </sumario>
        public void Dispose()
        {
            // Nota: O método Dispose não pode ser async, então usamos .GetAwaiter().GetResult() 
            // para execução síncrona do DropDatabase.
            var client = new MongoClient(MongoConnectionString);
            client.DropDatabase(TestDatabaseName);
        }
    }
}