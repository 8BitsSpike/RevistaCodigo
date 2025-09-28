namespace Media.DbContext.Persistence
{
    public class MediaDatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DataBaseName { get; set; } = null!;
        public string MediaCollectionName { get; set; } = null!;
    }
}
