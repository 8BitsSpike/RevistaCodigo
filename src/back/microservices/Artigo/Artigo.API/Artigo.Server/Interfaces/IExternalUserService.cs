using Artigo.Server.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Artigo.Server.Interfaces
{
    /// <sumario>
    /// Contrato para o serviço que busca informações de usuários no sistema externo (UsuarioAPI).
    /// </sumario>
    public interface IExternalUserService
    {
        /// <sumario>
        /// Busca informações básicas de um usuário (nome e mídia) pelo UsuarioId.
        /// </sumario>
        /// <param name="usuarioId">O ID do usuário externo (obtido do token ou entidade).</param>
        Task<ExternalUserDTO?> GetUserByIdAsync(string usuarioId);

        /// <sumario>
        /// Busca informações básicas de múltiplos usuários em lote.
        /// Crucial para otimizar o desempenho de DataLoaders (N+1).
        /// </sumario>
        /// <param name="usuarioIds">Lista de IDs de usuários externos.</param>
        Task<IReadOnlyList<ExternalUserDTO>> GetUsersByIdsAsync(IReadOnlyList<string> usuarioIds);
    }
}