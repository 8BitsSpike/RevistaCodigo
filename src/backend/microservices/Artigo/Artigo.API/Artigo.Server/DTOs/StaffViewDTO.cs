using Artigo.Intf.Enums;

namespace Artigo.Server.DTOs
{
    /// <sumario>
    /// DTO para o "Staff View" format.
    /// Contém informações públicas sobre um membro da equipe.
    /// </sumario>
    public class StaffViewDTO
    {
        public string UsuarioId { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public FuncaoTrabalho Job { get; set; }
        public bool IsActive { get; set; }
    }
}