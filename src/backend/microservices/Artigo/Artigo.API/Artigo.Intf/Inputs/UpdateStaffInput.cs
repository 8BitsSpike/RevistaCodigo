using Artigo.Intf.Enums;
using System.ComponentModel.DataAnnotations;

namespace Artigo.Intf.Inputs
{
    /// <sumario>
    /// Classe de entrada para a atualização de um registro de Staff.
    /// Define os campos que podem ser modificados. Campos nulos serão ignorados.
    /// </sumario>
    public class UpdateStaffInput
    {
        /// <sumario>
        /// O ID do usuário (UsuarioId) do staff a ser atualizado.
        /// </sumario>
        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        /// <sumario>
        /// Opcional: A nova função de trabalho para o membro.
        /// </sumario>
        public FuncaoTrabalho? Job { get; set; }

        /// <sumario>
        /// Opcional: O novo status de atividade (para aposentar/reinstaurar).
        /// </sumario>
        public bool? IsActive { get; set; }
    }
}