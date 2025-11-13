using Usuario.Intf.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Usuario.DbContext.Persistence
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly UsuarioDatabaseSettings _settings;

        public MongoDbContext(IOptions<UsuarioDatabaseSettings> settings)
        {
            _settings = settings.Value;

            if (string.IsNullOrEmpty(_settings?.ConnectionString))
            {
                throw new InvalidOperationException(
                    "MongoDB configuration failed! ConnectionString is missing or null. Check 'UsuarioDatabase' section in appsettings.json.");
            }

            var client = new MongoClient(_settings.ConnectionString);
            _database = client.GetDatabase(_settings.DataBaseName);
        }

        public IMongoCollection<Usuario.Intf.Models.Usuario> Usuarios =>
            _database.GetCollection<Usuario.Intf.Models.Usuario>(_settings.UsuarioCollectionName);

    }
}