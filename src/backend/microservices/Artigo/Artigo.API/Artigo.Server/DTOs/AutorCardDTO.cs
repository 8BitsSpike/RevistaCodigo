using System.Collections.Generic;

namespace Artigo.Server.DTOs
{
    /// <sumario>
    /// DTO para o "Autor Card" format.
    /// Contém informações públicas do autor e uma lista resolvida de seus trabalhos.
    /// </sumario>
    public class AutorCardDTO
    {
        public string Id { get; set; } = string.Empty;
        public string UsuarioId { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;

        // IDs dos trabalhos (para o resolver buscar os títulos)
        public List<string> ArtigoWorkIds { get; set; } = [];

        // Lista de trabalhos resolvidos (preenchido pelo resolver do GraphQL)
        public List<AutorTrabalhoDTO> Trabalhos { get; set; } = [];
    }

    /// <sumario>
    /// DTO aninhado para representar um trabalho na lista de um AutorCard.
    /// </sumario>
    public class AutorTrabalhoDTO
    {
        public string ArtigoId { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
    }
}