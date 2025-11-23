using System.Collections.Generic;
using System.Threading.Tasks;
using Artigo.Intf.Entities;
using Artigo.Intf.Enums;

namespace Artigo.Intf.Interfaces
{
    /// <sumario>
    /// Define o contrato para operações de persistência de dados na coleção Staff.
    /// É crucial para a camada de Service realizar a validação de autorização.
    /// </sumario>
    public interface IStaffRepository
    {
        /// <sumario>
        /// Retorna o registro de Staff pelo ID do Usuário (UsuarioId).
        /// </sumario>
        Task<Staff?> GetByUsuarioIdAsync(string usuarioId, object? sessionHandle = null);

        /// <sumario>
        /// Retorna o registro de Staff pelo ID local.
        /// </sumario>
        Task<Staff?> GetByIdAsync(string id, object? sessionHandle = null);

        /// <sumario>
        /// Retorna uma lista de todos os membros da Staff com uma funcao especifica.
        /// </sumario>
        Task<IReadOnlyList<Staff>> GetByRoleAsync(FuncaoTrabalho role, object? sessionHandle = null);

        /// <sumario>
        /// Retorna todos os registros de Staff, com suporte a paginação.
        /// </sumario>
        Task<IReadOnlyList<Staff>> GetAllAsync(int pagina, int tamanho, object? sessionHandle = null);

        /// <sumario>
        /// Adiciona um novo membro à equipe Staff.
        /// </sumario>
        Task AddAsync(Staff staffMember, object? sessionHandle = null);

        /// <sumario>
        /// Atualiza o registro de um membro da Staff (e.g., mudanca de FuncaoTrabalho).
        /// </sumario>
        Task<bool> UpdateAsync(Staff staffMember, object? sessionHandle = null);

        /// <sumario>
        /// Remove um membro da equipe Staff.
        /// </sumario>
        Task<bool> DeleteAsync(string id, object? sessionHandle = null);
    }
}