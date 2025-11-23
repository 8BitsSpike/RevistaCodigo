using MongoDB.Driver;
using Artigo.DbContext.PersistenceModels;
using System.ComponentModel.DataAnnotations;

namespace Artigo.DbContext.Config
{
    // ... (Classe MongoDBSettings segue inalterada, se houver)
}

namespace Artigo.DbContext.Data
{
    using Artigo.DbContext.Config;
    using Artigo.DbContext.Interfaces;

    /// <sumario>
    /// Implementação do contexto de dados. Centraliza a conexão do MongoClient
    /// e a inicialização de todas as coleções do projeto.
    /// </sumario>
    public class MongoDbContext : Artigo.DbContext.Interfaces.IMongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IMongoClient client, string databaseName)
        {
            _database = client.GetDatabase(databaseName);
        }

        // FIX: Mapeando explicitamente para os nomes das coleções no banco (sem 'Model')
        public IMongoCollection<ArtigoModel> Artigos => _database.GetCollection<ArtigoModel>("Artigo");
        public IMongoCollection<AutorModel> Autores => _database.GetCollection<AutorModel>("Autor");
        public IMongoCollection<EditorialModel> Editoriais => _database.GetCollection<EditorialModel>("Editorial");
        public IMongoCollection<InteractionModel> Interactions => _database.GetCollection<InteractionModel>("Interaction");
        public IMongoCollection<ArtigoHistoryModel> ArtigoHistories => _database.GetCollection<ArtigoHistoryModel>("ArtigoHistory");
        public IMongoCollection<PendingModel> Pendings => _database.GetCollection<PendingModel>("Pending");
        public IMongoCollection<StaffModel> Staffs => _database.GetCollection<StaffModel>("Staff");

        // Nota: Verifique se no banco é "Volume" (maiúsculo) ou "volume" (minúsculo) como você mencionou.
        // Assumindo "Volume" para manter o padrão das outras. Se for minúsculo, altere para "volume".
        public IMongoCollection<VolumeModel> Volumes => _database.GetCollection<VolumeModel>("Volume");
    }
}