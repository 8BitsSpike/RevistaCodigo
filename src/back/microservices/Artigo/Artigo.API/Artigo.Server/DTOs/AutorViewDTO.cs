namespace Artigo.Server.DTOs
{
    /// <sumario>
    /// Data Transfer Object (DTO) para o formato 'Autor Format'.
    /// Expõe informações públicas sobre um autor.
    /// </sumario>
    public class AutorViewDTO
    {
        public string UsuarioId { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}