using System.Collections.Generic;
using System.Threading.Tasks;
using Artigo.Intf.Entities;
using Artigo.Intf.Enums;

namespace Artigo.Intf.Interfaces
{
    /// <sumario>
    /// Define o contrato para operacoes de persistencia de dados na colecao Staff.
    /// É crucial para a camada de Service realizar a validacao de autorizacao.
    /// </sumario>
    public interface IStaffRepository
    {
        /// <sumario>
        /// Retorna o registro de Staff pelo ID do Usuário (UsuarioId).
        /// Usado primariamente para checar a JobRole de um usuario antes de executar uma Query/Mutation.
        /// </sumario>
        /// <param name="usuarioId">O ID do usuário externo (do UsuarioApi).</param>
        Task<Staff?> GetByUsuarioIdAsync(string usuarioId);

        /// <sumario>
        /// Retorna o registro de Staff pelo ID local.
        /// </sumario>
        /// <param name="id">O ID local (ObjectId) do registro de Staff.</param>
        Task<Staff?> GetByIdAsync(string id);

        /// <sumario>
        /// Retorna uma lista de todos os membros da Staff com uma funcao especifica.
        /// Útil para listar editores disponíveis para atribuicao.
        /// </sumario>
        /// <param name="role">A funcao (JobRole) para filtrar.</param>
        Task<IReadOnlyList<Staff>> GetByRoleAsync(JobRole role);

        /// <sumario>
        /// Adiciona um novo membro à equipe Staff.
        /// </sumario>
        /// <param name="staffMember">O objeto Staff a ser criado.</param>
        Task AddAsync(Staff staffMember);

        /// <sumario>
        /// Atualiza o registro de um membro da Staff (e.g., mudanca de JobRole).
        /// Requer permissao de Administrador.
        /// </sumario>
        /// <param name="staffMember">O objeto Staff com os dados atualizados.</param>
        Task<bool> UpdateAsync(Staff staffMember);

        /// <sumario>
        /// Remove um membro da equipe Staff.
        /// </summary>
        /// <param name="id">O ID do registro de Staff a ser removido.</param>
        Task<bool> DeleteAsync(string id);
    }
}
