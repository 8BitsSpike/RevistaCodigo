using Media.DbContext.Persistence;
using Media.Intf.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Media.DbContext
{
    public class MongoDbContext
    {
        private readonly MongoClient _client;
        private readonly IMongoDatabase _database;

        public MongoDbContext(IOptions<MediaDatabaseSettings> settings)
        {
            _client = new MongoClient(settings.Value.ConnectionString);
            _database = _client.GetDatabase(settings.Value.DataBaseName);
        }

        public IMongoCollection<Midia> Midias =>
            _database.GetCollection<Midia>("Media");
    }
}