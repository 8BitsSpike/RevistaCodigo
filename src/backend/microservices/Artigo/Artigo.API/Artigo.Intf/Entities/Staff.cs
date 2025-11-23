using Artigo.Intf.Enums;

namespace Artigo.Intf.Entities
{
    /// <sumario>
    /// Representa um registro local da equipe editorial (staff) para fins de autorização.
    /// Funciona como um link de permissão para o ID do usuário externo (UsuarioApi).
    /// </sumario>
    public class Staff
    {
        // Identificador do Dominio.
        public string Id { get; set; } = string.Empty;

        // O ID do usuário no sistema externo (UsuarioApi) ao qual esta permissão se refere.
        public string UsuarioId { get; set; } = string.Empty;

        // Nome de exibição do usuário (obtido da mutação, vindo do UsuarioAPI).
        public string Nome { get; set; } = string.Empty;

        // URL da mídia de perfil/avatar do usuário (obtido da mutação).
        public string Url { get; set; } = string.Empty;

        // A função principal do membro da equipe, usada para verificação de permissão.
        public FuncaoTrabalho Job { get; set; } = FuncaoTrabalho.EditorBolsista;

        // Identificador do ciclo de vida: Se este Staff esta ativo ou inativo.
        public bool IsActive { get; set; } = true;
    }
}