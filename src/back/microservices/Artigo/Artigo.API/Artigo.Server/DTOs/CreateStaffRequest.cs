using Artigo.Intf.Enums;
using System.ComponentModel.DataAnnotations;

namespace Artigo.Server.DTOs
{
    /// <sumario>
    /// Data Transfer Object (DTO) de entrada para a Mutação de criação de um novo registro Staff.
    /// Usado por Administradores e Editores Chefes para promover um usuário a membro da equipe.
    /// </sumario>
    public class CreateStaffRequest
    {
        /// <sumario>
        /// O ID do usuário no sistema externo (UsuarioAPI) a ser promovido.
        /// </sumario>
        [Required(ErrorMessage = "O ID do usuário externo é obrigatório.")]
        public string UsuarioId { get; set; } = string.Empty;

        /// <sumario>
        /// A função inicial de trabalho a ser atribuída ao novo membro da equipe.
        /// </sumario>
        [Required(ErrorMessage = "A função de trabalho é obrigatória.")]
        public FuncaoTrabalho Job { get; set; } = FuncaoTrabalho.EditorBolsista;

        /// <sumario>
        /// Nome de exibição do usuário (para denormalização).
        /// </sumario>
        [Required(ErrorMessage = "O nome do usuário é obrigatório.")]
        [MaxLength(150, ErrorMessage = "O nome não pode exceder 150 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        /// <sumario>
        /// URL da foto de perfil do usuário (para denormalização).
        /// </sumario>
        [MaxLength(500, ErrorMessage = "A URL não pode exceder 500 caracteres.")]
        public string Url { get; set; } = string.Empty;
    }
}