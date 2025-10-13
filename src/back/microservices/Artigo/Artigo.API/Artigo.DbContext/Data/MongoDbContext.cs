using Artigo.Intf.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.ComponentModel.DataAnnotations;
using Artigo.Intf.Entities; // Adicionado para referenciar as Entidades de Dominio
using Artigo.DbContext.PersistenceModels; // Necessário para referenciar os Persistence Models

namespace Artigo.DbContext.Config
{
    // ... (MongoDBSettings class remains the same)
}

namespace Artigo.DbContext.Data
{
    using Artigo.DbContext.Config;

    /// <sumario>
    /// Implementação do contexto de dados. Centraliza a conexão do MongoClient
    /// e a inicialização de todas as coleções do projeto.
    /// </sumario>
    public class MongoDbContext : IMongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IMongoClient client, string databaseName)
        {
            _database = client.GetDatabase(databaseName);
        }

        // --- Implementação das Coleções com Cast Explícito ---
        // A implementação GetCollection<TModel> é castada para IMongoCollection<TEntity>
        // O driver do MongoDB lida com isso corretamente, tratando o ArtigoModel como Artigo.

        public IMongoCollection<Artigo.Intf.Entities.Artigo> Artigos => (IMongoCollection<Artigo.Intf.Entities.Artigo>)_database.GetCollection<ArtigoModel>(nameof(ArtigoModel));
        public IMongoCollection<Autor> Autores => (IMongoCollection<Autor>)_database.GetCollection<AutorModel>(nameof(AutorModel));
        public IMongoCollection<Editorial> Editoriais => (IMongoCollection<Editorial>)_database.GetCollection<EditorialModel>(nameof(EditorialModel));
        public IMongoCollection<Interaction> Interactions => (IMongoCollection<Interaction>)_database.GetCollection<InteractionModel>(nameof(InteractionModel));
        public IMongoCollection<ArtigoHistory> ArtigoHistories => (IMongoCollection<ArtigoHistory>)_database.GetCollection<ArtigoHistoryModel>(nameof(ArtigoHistoryModel));
        public IMongoCollection<Pending> Pendings => (IMongoCollection<Pending>)_database.GetCollection<PendingModel>(nameof(PendingModel));
        public IMongoCollection<Staff> Staffs => (IMongoCollection<Staff>)_database.GetCollection<StaffModel>(nameof(StaffModel));
        public IMongoCollection<Volume> Volumes => (IMongoCollection<Volume>)_database.GetCollection<VolumeModel>(nameof(VolumeModel));
    }
}