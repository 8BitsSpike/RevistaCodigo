using Artigo.Intf.Enums;
using Artigo.Intf.Interfaces;
using Artigo.Intf.Entities;
using Artigo.Server.DTOs;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Artigo.API.GraphQL.Mutations
{
    /// <sumario>
    /// Define os métodos de modificação de dados (Mutations) do GraphQL relacionados à entidade Artigo.
    /// </sumario>
    [ExtendObjectType(Name = "Mutation")]
    public class ArtigoMutations
    {
        /// <sumario>
        /// Cria um novo artigo e o registro editorial associado.
        /// REGRA: Qualquer usuário autenticado pode criar um artigo.
        /// </sumario>
        [Mutation]
        [Authorize]
        public async Task<ArtigoDTO> CreateArtigoAsync(
            CreateArtigoRequest input,
            [Service] IArtigoService artigoService,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            // O service layer lida com a lógica de inicialização e criação de entidades.
            return await artigoService.CreateArtigoAsync(input, currentUsuarioId);
        }

        /// <sumario>
        /// Atualiza os metadados (Título, Resumo) de um artigo existente.
        /// REGRA: Requer autorização verificada pelo service (Autor/Staff).
        /// </sumario>
        [Mutation]
        [Authorize]
        public async Task<ArtigoDTO> UpdateArtigoMetadataAsync(
            string id,
            ArtigoDTO input,
            [Service] IArtigoService artigoService,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            input.Id = id; // Garante que a ID está sendo atualizada no DTO de input.

            var success = await artigoService.UpdateArtigoMetadataAsync(input, currentUsuarioId);

            if (success)
            {
                // Busca e retorna a versão atualizada e autorizada do artigo.
                // Usamos GetArtigoForEditorialAsync para garantir que o usuário ainda tem permissão para vê-lo.
                return await artigoService.GetArtigoForEditorialAsync(id, currentUsuarioId)
                       ?? throw new InvalidOperationException("Artigo atualizado, mas falha ao recuperá-lo.");
            }

            throw new InvalidOperationException("Falha ao atualizar metadados do artigo. Verifique a ID ou permissões.");
        }

        /// <sumario>
        /// Cria um novo comentário público em um artigo publicado.
        /// </summary>
        [Mutation]
        [Authorize] // Comentários públicos requerem que o usuário esteja logado.
        public async Task<Interaction> CreatePublicCommentAsync(
            string artigoId,
            string content,
            string? parentCommentId, // Para respostas aninhadas
            [Service] IArtigoService artigoService,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            var newComment = new Interaction
            {
                UsuarioId = currentUsuarioId,
                Content = content,
                Type = InteractionType.ComentarioPublico,
                ParentCommentId = parentCommentId // Pode ser null
            };

            return await artigoService.CreatePublicCommentAsync(artigoId, newComment, parentCommentId);
        }

        /// <sumario>
        /// Cria um novo comentário editorial interno.
        /// REGRA: O service fará a verificação se o usuário é membro da equipe editorial.
        /// </summary>
        [Mutation]
        [Authorize] // Comentários editoriais requerem autenticação (e autorização no service).
        public async Task<Interaction> CreateEditorialCommentAsync(
            string artigoId,
            string content,
            [Service] IArtigoService artigoService,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            var newComment = new Interaction
            {
                UsuarioId = currentUsuarioId,
                Content = content,
                Type = InteractionType.ComentarioEditorial,
                ParentCommentId = null // Comentários editoriais não suportam aninhamento neste modelo
            };

            // O service verifica se o currentUsuarioId é parte do EditorialTeam
            return await artigoService.CreateEditorialCommentAsync(artigoId, newComment, currentUsuarioId);
        }
    }
}