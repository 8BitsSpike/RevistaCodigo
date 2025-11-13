using System.ComponentModel.DataAnnotations;

namespace Artigo.Server.DTOs
{
    /// <sumario>
    /// DTO de entrada para uma entrada de Mídia (URL, Alt Text, ID).
    /// Usado na mutação de criação de artigo.
    /// </sumario>
    public class MidiaEntryInputDTO
    {
        [Required(ErrorMessage = "O ID da mídia é obrigatório.")]
        public string MidiaID { get; set; } = string.Empty;

        [Required(ErrorMessage = "A URL da mídia é obrigatória.")]
        [Url(ErrorMessage = "A URL fornecida não é válida.")]
        public string Url { get; set; } = string.Empty;

        [Required(ErrorMessage = "O texto alternativo (Alt) é obrigatório.")]
        [MaxLength(250, ErrorMessage = "O texto alternativo não pode exceder 250 caracteres.")]
        public string Alt { get; set; } = string.Empty;
    }
}