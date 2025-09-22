using Usuario.DbContext.Persistence;
using Usuario.Intf.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Usuario.DbContext
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly UsuarioDatabaseSettings _settings;

        public MongoDbContext(IOptions<UsuarioDatabaseSettings> settings)
        {
            _settings = settings.Value;
            var client = new MongoClient(_settings.ConnectionString);
            _database = client.GetDatabase(_settings.DataBaseName);
        }

        public IMongoCollection<Usuar> Usuarios =>
            _database.GetCollection<Usuar>(_settings.UsuarioCollectionName);

    }
}
