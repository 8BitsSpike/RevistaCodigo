using Media.DbContext.Persistence;
using Media.Intf.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Media.DbContext
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly MediaDatabaseSettings _settings;

        public MongoDbContext(IOptions<MediaDatabaseSettings> settings)
        {
            _settings = settings.Value;
            var client = new MongoClient(_settings.ConnectionString);
            _database = client.GetDatabase(_settings.DataBaseName);
        }

        public IMongoCollection<Media> Medias =>
            _database.GetCollection<Media>(_settings.MediaCollectionName);

    }
}
