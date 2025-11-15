using Artigo.Intf.Enums;
using Artigo.Intf.Interfaces;
using Artigo.Intf.Entities;
using Artigo.Server.DTOs;
using Artigo.API.GraphQL.Inputs;
using Artigo.Intf.Inputs;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Artigo.API.GraphQL.Mutations
{
    /// <summary>
    /// Define os métodos de modificação de dados (Mutations), utilizando Constructor Injection.
    /// Esta classe é mapeada manually em ArtigoMutationType.cs para evitar erros.
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
            string commentary,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            // Mapeia DTOs para Entidades antes de chamar o serviço
            var newArtigo = _mapper.Map<Artigo.Intf.Entities.Artigo>(input);
            var autores = _mapper.Map<List<Autor>>(input.Autores);

            // Mapeia a lista completa de DTOs de mídia para entidades de mídia
            var midiasCompletas = _mapper.Map<List<MidiaEntry>>(input.Midias);

            var createdArtigo = await _artigoService.CreateArtigoAsync(newArtigo, input.Conteudo, midiasCompletas, autores, currentUsuarioId, commentary);
            return _mapper.Map<ArtigoDTO>(createdArtigo);
        }

        public async Task<ArtigoDTO> UpdateArtigoMetadataAsync(
            string id,
            UpdateArtigoMetadataInput input,
            string commentary,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            // Passa o DTO de input diretamente para o serviço.
            // O serviço irá lidar com a lógica de atualização parcial (Bug Fix).
            var success = await _artigoService.AtualizarMetadadosArtigoAsync(id, input, currentUsuarioId, commentary);

            if (success)
            {
                var updatedEntity = await _artigoService.ObterArtigoParaEditorialAsync(id, currentUsuarioId)
                                       ?? throw new InvalidOperationException("Artigo atualizado, mas falha ao recuperá-lo.");
                return _mapper.Map<ArtigoDTO>(updatedEntity);
            }

            throw new InvalidOperationException("Falha ao atualizar metadados do artigo. Verifique a ID ou permissões.");
        }

        /// <summary>
        /// Atualiza o conteúdo e a lista de mídias de um artigo.
        /// </summary>
        public async Task<bool> AtualizarConteudoArtigoAsync(
            string artigoId,
            string newContent,
            List<MidiaEntryInputDTO> midias, // Recebe DTOs de input
            string commentary,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            // Mapeia os DTOs de input para entidades
            var midiasEntidades = _mapper.Map<List<MidiaEntry>>(midias);

            return await _artigoService.AtualizarConteudoArtigoAsync(
                artigoId,
                newContent,
                midiasEntidades, // Passa a lista de entidades
                currentUsuarioId,
                commentary
            );
        }

        /// <summary>
        /// Atualiza a equipe editorial (revisores, corretores) de um artigo.
        /// </summary>
        public async Task<Editorial> AtualizarEquipeEditorialAsync(
            string artigoId,
            EditorialTeam teamInput, // Recebe a entidade EditorialTeam mapeada do InputType
            string commentary,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            return await _artigoService.AtualizarEquipeEditorialAsync(
                artigoId,
                teamInput,
                currentUsuarioId,
                commentary
            );
        }

        public async Task<Interaction> CreatePublicCommentAsync(
            string artigoId,
            string content,
            string usuarioNome,
            string? parentCommentId,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            var newComment = new Artigo.Intf.Entities.Interaction
            {
                UsuarioId = currentUsuarioId,
                UsuarioNome = usuarioNome,
                Content = content,
                Type = TipoInteracao.ComentarioPublico,
                ParentCommentId = parentCommentId
            };

            return await _artigoService.CriarComentarioPublicoAsync(artigoId, newComment, parentCommentId);
        }

        public async Task<Interaction> CreateEditorialCommentAsync(
            string artigoId,
            string content,
            string usuarioNome,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            var newComment = new Artigo.Intf.Entities.Interaction
            {
                UsuarioId = currentUsuarioId,
                UsuarioNome = usuarioNome,
                Content = content,
                Type = TipoInteracao.ComentarioEditorial,
                ParentCommentId = null
            };

            return await _artigoService.CriarComentarioEditorialAsync(artigoId, newComment, currentUsuarioId);
        }

        /// <summary>
        /// Atualiza o conteúdo de uma interação (comentário).
        /// </summary>
        public async Task<Interaction> AtualizarInteracaoAsync(
            string interacaoId,
            string newContent,
            string commentary,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? throw new UnauthorizedAccessException("Usuário deve estar autenticado.");
            return await _artigoService.AtualizarInteracaoAsync(interacaoId, newContent, currentUsuarioId, commentary);
        }

        /// <summary>
        /// Deleta uma interação (comentário).
        /// </summary>
        public async Task<bool> DeletarInteracaoAsync(
            string interacaoId,
            string commentary,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? throw new UnauthorizedAccessException("Usuário deve estar autenticado.");
            return await _artigoService.DeletarInteracaoAsync(interacaoId, currentUsuarioId, commentary);
        }

        /// <summary>
        /// Metodo para criar um novo registro de Staff para um usuário e define sua função de trabalho.
        /// </summary>
        public async Task<Staff> CriarNovoStaffAsync(
            CreateStaffRequest input,
            string commentary,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            // Mapeia DTO para Entidade
            var novoStaff = _mapper.Map<Staff>(input);

            return await _artigoService.CriarNovoStaffAsync(
                novoStaff,
                currentUsuarioId,
                commentary
            );
        }

        /// <summary>
        /// (NOVO) Metodo para atualizar o registro de um Staff (promover, demover, aposentar).
        /// </summary>
        public async Task<StaffViewDTO> AtualizarStaffAsync(
            UpdateStaffInput input,
            string commentary,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            var updatedStaffEntity = await _artigoService.AtualizarStaffAsync(
                input,
                currentUsuarioId,
                commentary
            );

            // Mapeia a entidade Staff (retornada pelo serviço) para o DTO de visualização
            return _mapper.Map<StaffViewDTO>(updatedStaffEntity);
        }

        /// <summary>
        /// Metodo para criar um novo registro de Volume (Edição de revista).
        /// </summary>
        public async Task<Volume> CriarVolumeAsync(
            CreateVolumeRequest input,
            string commentary,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            var novoVolume = _mapper.Map<Volume>(input);

            return await _artigoService.CriarVolumeAsync(
                novoVolume,
                currentUsuarioId,
                commentary
            );
        }

        /// <summary>
        /// Metodo para atualizar os metadados de um Volume (Edição de revista).
        /// </summary>
        public async Task<bool> AtualizarMetadadosVolumeAsync(
            string volumeId,
            UpdateVolumeMetadataInput input,
            string commentary,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            return await _artigoService.AtualizarMetadadosVolumeAsync(
                volumeId,
                input,
                currentUsuarioId,
                commentary
            );
        }

        // =========================================================================
        // *** MUTAÇÕES (StaffComentario) ***
        // =========================================================================

        public async Task<ArtigoHistory> AddStaffComentarioAsync(
            string historyId,
            string comment,
            string? parent,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? throw new UnauthorizedAccessException("Usuário deve estar autenticado.");
            return await _artigoService.AddStaffComentarioAsync(historyId, currentUsuarioId, comment, parent);
        }

        public async Task<ArtigoHistory> UpdateStaffComentarioAsync(
            string historyId,
            string comentarioId,
            string newContent,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? throw new UnauthorizedAccessException("Usuário deve estar autenticado.");
            return await _artigoService.UpdateStaffComentarioAsync(historyId, comentarioId, newContent, currentUsuarioId);
        }

        public async Task<ArtigoHistory> DeleteStaffComentarioAsync(
            string historyId,
            string comentarioId,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? throw new UnauthorizedAccessException("Usuário deve estar autenticado.");
            return await _artigoService.DeleteStaffComentarioAsync(historyId, comentarioId, currentUsuarioId);
        }

        // =========================================================================
        // *** MÉTODO (Manual Pending) ***
        // =========================================================================

        /// <summary>
        /// Cria manually uma nova requisição pendente (Admin).
        /// </summary>
        public async Task<Pending> CriarRequisicaoPendenteAsync(
            Pending input,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            // O input já é a entidade 'Pending'
            return await _artigoService.CriarRequisicaoPendenteAsync(input, currentUsuarioId);
        }

        /// <summary>
        /// (NOVO) Resolve uma requisição pendente (Aprova ou Rejeita).
        /// </summary>
        public async Task<bool> ResolverRequisicaoPendenteAsync(
            string pendingId,
            bool isApproved,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? throw new UnauthorizedAccessException("Usuário deve estar autenticado.");
            return await _artigoService.ResolverRequisicaoPendenteAsync(pendingId, isApproved, currentUsuarioId);
        }
    }
}