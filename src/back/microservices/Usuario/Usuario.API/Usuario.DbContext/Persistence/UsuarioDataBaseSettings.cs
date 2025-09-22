namespace Usuario.DbContext.Persistence
{
    public class UsuarioDatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DataBaseName { get; set; } = null!;
        public string UsuarioCollectionName { get; set; } = null!;
    }
}
