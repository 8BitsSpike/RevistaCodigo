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
using System.Collections.Generic; // Added for List<string> needed in ArtigoEntity creation

namespace Artigo.API.GraphQL.Mutations
{
    /// <sumario>
    /// Define os métodos de modificação de dados (Mutations) do GraphQL relacionados à entidade Artigo.
    /// </sumario>
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
            [Service] AutoMapper.IMapper mapper, // FIX: Inject IMapper
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            // 1. Map DTO input (which includes Title, Resume, Type) to the Domain Entity (Artigo)
            var newArtigo = mapper.Map<Artigo.Intf.Entities.Artigo>(input);

            // 2. Call service with the three required arguments (Entity, Content, UsuarioId)
            var createdArtigo = await artigoService.CreateArtigoAsync(
                newArtigo,
                input.Content, // Extracted Content (second argument)
                currentUsuarioId // Extracted UsuarioId (third argument)
            );

            // 3. Map the returned Domain Entity back to the output DTO for the GraphQL response
            return mapper.Map<ArtigoDTO>(createdArtigo);
        }

        /// <sumario>
        /// Atualiza os metadados (Título, Resumo) de um artigo existente.
        /// REGRA: Requer autorização verificada pelo service (Autor/Staff).
        /// </sumario>
        [Mutation]
        [Authorize]
        public async Task<ArtigoDTO> UpdateArtigoMetadataAsync(
            string id,
            UpdateArtigoMetadataInput input,
            [Service] IArtigoService artigoService,
            [Service] AutoMapper.IMapper mapper, // FIX: Inject IMapper
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            // Mapear o Update Input para a Entidade Artigo (apenas campos parciais)
            var artigoEntity = new Artigo.Intf.Entities.Artigo
            {
                Id = id,
                Titulo = input.Titulo ?? string.Empty,
                Resumo = input.Resumo ?? string.Empty,
                Tipo = input.Tipo ?? ArtigoTipo.Artigo,
                AutorIds = input.AutorIds ?? new List<string>(),
                AutorReference = input.AutorReference ?? new List<string>()
            };

            var success = await artigoService.UpdateArtigoMetadataAsync(artigoEntity, currentUsuarioId);

            if (success)
            {
                var updatedEntity = await artigoService.GetArtigoForEditorialAsync(id, currentUsuarioId)
                                       ?? throw new InvalidOperationException("Artigo atualizado, mas falha ao recuperá-lo.");

                // FIX: Map the returned Entity back to DTO
                return mapper.Map<ArtigoDTO>(updatedEntity);
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