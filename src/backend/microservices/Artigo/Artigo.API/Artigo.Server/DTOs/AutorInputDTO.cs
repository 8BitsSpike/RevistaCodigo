using System.ComponentModel.DataAnnotations;

namespace Artigo.Server.DTOs
{
    /// <sumario>
    /// Data Transfer Object (DTO) de entrada para um Autor.
    /// Usado em mutações (como CreateArtigo) para fornecer os dados denormalizados do autor.
    /// </sumario>
    public class AutorInputDTO
    {
        [Required(ErrorMessage = "O ID do usuário externo é obrigatório.")]
        public string UsuarioId { get; set; } = string.Empty;

        [Required(ErrorMessage = "O nome do usuário é obrigatório.")]
        [MaxLength(150, ErrorMessage = "O nome não pode exceder 150 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "A URL não pode exceder 500 caracteres.")]
        public string Url { get; set; } = string.Empty; // URL para foto de perfil, etc.
    }
}