using Artigo.Intf.Enums;

namespace Artigo.Intf.Entities
{
    /// <sumario>
    /// Representa um registro local da equipe editorial (staff) para fins de autorizacao.
    /// Funciona como um link de permissao para o ID do usuário externo (UsuarioApi).
    /// </sumario>
    public class Staff
    {
        // Identificador do Dominio.
        public string Id { get; set; } = string.Empty;

        // O ID do usuário no sistema externo (UsuarioApi) ao qual esta permissao se refere.
        public string UsuarioId { get; set; } = string.Empty;

        // A funcao principal do membro da equipe, usada para verificacao de permissao.
        public FuncaoTrabalho Job { get; set; } = FuncaoTrabalho.EditorBolsista; // CORRIGIDO: FuncaoTrabalho.EditorBolsista

        // Identificador do ciclo de vida: Se este Staff esta ativo ou inativo.
        public bool IsActive { get; set; } = true;
    }
}