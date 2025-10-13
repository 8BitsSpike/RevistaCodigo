using Artigo.Intf.Enums;
using Artigo.Intf.Interfaces;
using Artigo.Intf.Entities;
using Artigo.Server.DTOs;
using Artigo.API.GraphQL.Inputs;
using HotChocolate.Authorization;
using HotChocolate.Types;
using System.Security.Claims;
using System.Threading.Tasks;
using System;

namespace Artigo.API.GraphQL.Mutations
{
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
            // FIX: Usar o novo tipo de input específico para updates.
            UpdateArtigoMetadataInput input,
            [Service] IArtigoService artigoService,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            // FIX: Mapear o Update Input para o DTO de Domain/Service
            var artigoEntity = new Artigo.Intf.Entities.Artigo
            {
                Id = id,
                Titulo = input.Titulo ?? string.Empty, // A Entidade Artigo não permite nullables aqui,
                Resumo = input.Resumo ?? string.Empty, // então assumimos string.Empty se nulo for passado.
                Tipo = input.Tipo ?? ArtigoTipo.Artigo, // Se nulo, assumir um default para evitar erro de tipo.
                AutorIds = input.AutorIds ?? new List<string>(),
                AutorReference = input.AutorReference ?? new List<string>()
            };

            // O ArtigoService precisa ser ajustado para lidar com DTOs parciais ou
            // este método de construção da entidade Artigo precisa ser feito com o AutoMapper.

            // NOTE: Para fins de compilação, vamos passar a Entidade Artigo para o service.
            // O service deve lidar com a busca do objeto existente e a aplicação dos campos não-nulos.
            var success = await artigoService.UpdateArtigoMetadataAsync(artigoEntity, currentUsuarioId);

            if (success)
            {
                return await artigoService.GetArtigoForEditorialAsync(id, currentUsuarioId)
                       ?? throw new InvalidOperationException("Artigo atualizado, mas falha ao recuperá-lo.");
            }

            throw new InvalidOperationException("Falha ao atualizar metadados do artigo. Verifique a ID ou permissões.");
        }

        /// <sumario>
        /// Cria um novo comentário público em um artigo publicado.
        /// </summary>
        [Mutation]
        [Authorize]
        public async Task<Interaction> CreatePublicCommentAsync(
            string artigoId,
            string content,
            string? parentCommentId,
            [Service] IArtigoService artigoService,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            var newComment = new Artigo.Intf.Entities.Interaction
            {
                UsuarioId = currentUsuarioId,
                Content = content,
                Type = Artigo.Intf.Enums.InteractionType.ComentarioPublico,
                ParentCommentId = parentCommentId
            };

            return await artigoService.CreatePublicCommentAsync(artigoId, newComment, parentCommentId);
        }

        /// <sumario>
        /// Cria um novo comentário editorial interno.
        /// REGRA: O service fará a verificação se o usuário é membro da equipe editorial.
        /// </summary>
        [Mutation]
        [Authorize]
        public async Task<Interaction> CreateEditorialCommentAsync(
            string artigoId,
            string content,
            [Service] IArtigoService artigoService,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            var newComment = new Artigo.Intf.Entities.Interaction
            {
                UsuarioId = currentUsuarioId,
                Content = content,
                Type = Artigo.Intf.Enums.InteractionType.ComentarioEditorial,
                ParentCommentId = null
            };

            return await artigoService.CreateEditorialCommentAsync(artigoId, newComment, currentUsuarioId);
        }
    }
}