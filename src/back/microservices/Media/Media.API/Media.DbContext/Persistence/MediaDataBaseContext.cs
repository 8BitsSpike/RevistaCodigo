namespace Media.DbContext.Persistence
{
    public class MediaDatabaseSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DataBaseName { get; set; } = string.Empty;
        public string MediaCollectionName { get; set; } = string.Empty;
    }
}
