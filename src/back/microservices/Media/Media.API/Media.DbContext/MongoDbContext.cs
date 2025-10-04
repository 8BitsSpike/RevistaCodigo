using Media.DbContext.Persistence;
using Media.Intf.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;

namespace Media.DbContext
{
    public class MongoDbContext
    {
        private readonly MongoClient _client;
        private readonly IMongoDatabase _database;
        private readonly MediaDatabaseSettings _settings;
        private readonly ILogger<MongoDbContext> _logger;

        public MongoDbContext(IOptions<MediaDatabaseSettings> settingsOptions, ILogger<MongoDbContext> logger)
        {
            _settings = settingsOptions.Value;
            _logger = logger;

            try
            {
                if (string.IsNullOrEmpty(_settings.ConnectionString))
                {
                    throw new InvalidOperationException("MongoDB ConnectionString is missing from configuration.");
                }

                _client = new MongoClient(_settings.ConnectionString);
                _database = _client.GetDatabase(_settings.DataBaseName);

                _logger.LogInformation("MongoDB connection successful to database: {DatabaseName}", _settings.DataBaseName);
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "FATAL ERROR during MongoDbContext initialization. Check if MongoDB server is running.");
                throw new InvalidOperationException("Failed to initialize MongoDbContext.", ex);
            }
        }
        public IMongoCollection<Midia> Midias =>
            _database.GetCollection<Midia>(_settings.MediaCollectionName);
    }
}