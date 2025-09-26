namespace Usuario.DbContext.Persistence
{
    public class UsuarioDatabaseSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DataBaseName { get; set; } = string.Empty;
        public string UsuarioCollectionName { get; set; } = string.Empty;
    }
}
