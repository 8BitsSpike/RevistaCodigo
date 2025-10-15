using Artigo.Intf.Enums;
using Artigo.Intf.Interfaces;
using Artigo.Intf.Entities;
using Artigo.Server.DTOs;
using Artigo.API.GraphQL.Inputs;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace Artigo.API.GraphQL.Mutations
{
    /// <summary>
    /// Define os métodos de modificação de dados (Mutations), utilizando Constructor Injection.
    /// Esta classe é mapeada manualmente em ArtigoMutationType.cs.
    /// </summary>
    public class ArtigoMutation
    {
        private readonly IArtigoService _artigoService;
        private readonly AutoMapper.IMapper _mapper;

        public ArtigoMutation(IArtigoService artigoService, AutoMapper.IMapper mapper)
        {
            _artigoService = artigoService;
            _mapper = mapper;
        }

        public async Task<ArtigoDTO> CreateArtigoAsync(
            CreateArtigoRequest input,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? throw new UnauthorizedAccessException("Usuário deve estar autenticado.");
            var newArtigo = _mapper.Map<Artigo.Intf.Entities.Artigo>(input);

            var createdArtigo = await _artigoService.CreateArtigoAsync(newArtigo, input.Content, currentUsuarioId);
            return _mapper.Map<ArtigoDTO>(createdArtigo);
        }

        public async Task<ArtigoDTO> UpdateArtigoMetadataAsync(
            string id,
            UpdateArtigoMetadataInput input,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            var artigoEntity = new Artigo.Intf.Entities.Artigo
            {
                Id = id,
                Titulo = input.Titulo ?? string.Empty,
                Resumo = input.Resumo ?? string.Empty,
                Tipo = input.Tipo ?? ArtigoTipo.Artigo,
                AutorIds = input.AutorIds ?? new List<string>(),
                AutorReference = input.AutorReference ?? new List<string>()
            };

            var success = await _artigoService.UpdateArtigoMetadataAsync(artigoEntity, currentUsuarioId);

            if (success)
            {
                var updatedEntity = await _artigoService.GetArtigoForEditorialAsync(id, currentUsuarioId)
                                       ?? throw new InvalidOperationException("Artigo atualizado, mas falha ao recuperá-lo.");
                return _mapper.Map<ArtigoDTO>(updatedEntity);
            }

            throw new InvalidOperationException("Falha ao atualizar metadados do artigo. Verifique a ID ou permissões.");
        }

        public async Task<Interaction> CreatePublicCommentAsync(
            string artigoId,
            string content,
            string? parentCommentId,
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

            return await _artigoService.CreatePublicCommentAsync(artigoId, newComment, parentCommentId);
        }

        public async Task<Interaction> CreateEditorialCommentAsync(
            string artigoId,
            string content,
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

            return await _artigoService.CreateEditorialCommentAsync(artigoId, newComment, currentUsuarioId);
        }
    }
}
