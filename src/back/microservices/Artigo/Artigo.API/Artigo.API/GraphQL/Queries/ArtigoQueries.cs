using Artigo.Intf.Enums;
using Artigo.Intf.Interfaces;
using Artigo.Server.DTOs;
using HotChocolate.Authorization;
using HotChocolate.Data;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Artigo.API.GraphQL.Queries
{
    /// <sumario>
    /// Define os métodos de consulta (Query) do GraphQL relacionados à entidade Artigo.
    /// Utiliza o serviço da camada de Aplicação (ArtigoService).
    /// </sumario>
    public class ArtigoQueries
    {
        /// <sumario>
        /// Obtém um artigo pelo seu ID. O ArtigoService lida com as regras de autorização 
        /// (se está publicado ou se o usuário é membro da equipe editorial).
        /// </sumario>
        /// <param name="id">O ID do artigo (string ObjectId).</param>
        /// <param name="artigoService">Injeção do serviço de aplicação.</param>
        /// <param name="claims">Claims do usuário autenticado para verificar permissões.</param>
        /// <returns>O ArtigoDTO se encontrado e autorizado, ou null.</returns>
        [Query]
        public async Task<ArtigoDTO?> GetArtigoByIdAsync(
            string id,
            [Service] IArtigoService artigoService,
            ClaimsPrincipal claims)
        {
            // Extrai o ID do usuário (UsuarioId externo) do token JWT.
            // Assumimos que o UsuarioId é armazenado em uma claim chamada "sub" ou similar.
            var currentUsuarioId = claims.FindFirstValue("sub") ?? claims.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUsuarioId))
            {
                // Para consultas públicas (e.g., artigos published), o serviço deve lidar com a falta de ID.
                // Usamos string.Empty aqui, mas o serviço deve ser robusto a isso.
                currentUsuarioId = string.Empty;
            }

            return await artigoService.GetArtigoForEditorialAsync(id, currentUsuarioId);
        }

        /// <sumario>
        /// Obtém a lista de todos os artigos que estão em um status editorial específico.
        /// Requer que o usuário esteja autenticado e que o ArtigoService verifique a permissão Staff.
        /// </sumario>
        /// <param name="status">O status editorial a ser filtrado (e.g., InReview).</param>
        /// <param name="artigoService">Injeção do serviço de aplicação.</param>
        /// <param name="claims">Claims do usuário autenticado.</param>
        /// <returns>Lista de ArtigoDTOs.</returns>
        [Query]
        [Authorize] // Impede que usuários não autenticados chamem este método
        [UsePaging] // Permite paginação out-of-the-box (Cursor-based)
        [UseFiltering] // Permite filtros dinâmicos
        public async Task<IReadOnlyList<ArtigoDTO>> GetArtigosByStatusAsync(
            ArtigoStatus status,
            [Service] IArtigoService artigoService,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? claims.FindFirstValue(ClaimTypes.NameIdentifier);

            // Se a claim de usuário estiver ausente, a anotação [Authorize] já deve ter bloqueado a requisição.
            if (string.IsNullOrEmpty(currentUsuarioId))
            {
                return new List<ArtigoDTO>(); // Segurança caso a autorização falhe silenciosamente.
            }

            // O ArtigoService fará a verificação de permissão (se o usuário é Staff) e aplicará os filtros.
            return await artigoService.GetArtigosByStatusAsync(status, currentUsuarioId);
        }
    }
}