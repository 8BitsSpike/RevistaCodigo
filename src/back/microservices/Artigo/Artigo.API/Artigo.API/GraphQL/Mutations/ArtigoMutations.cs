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
    public class ArtigoMutation
    {
        private readonly IArtigoService _artigoService;
        private readonly AutoMapper.IMapper _mapper;

        public ArtigoMutation(IArtigoService artigoService, AutoMapper.IMapper mapper)
        {
            _artigoService = artigoService;
            _mapper = mapper;
        }

        // --- Helper para extrair ID do usuário de forma robusta ---
        private string GetUserId(ClaimsPrincipal claims)
        {
            return claims.FindFirstValue("sub")
                ?? claims.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new InvalidOperationException("ID do usuário (sub/nameid) não encontrado no token.");
        }

        // =========================================================================
        // ARTIGO CORE MUTATIONS
        // =========================================================================

        public async Task<ArtigoDTO> CreateArtigoAsync(
            CreateArtigoRequest input,
            string commentary,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = GetUserId(claims);

            var newArtigo = _mapper.Map<Artigo.Intf.Entities.Artigo>(input);
            var autores = _mapper.Map<List<Autor>>(input.Autores);
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
            var currentUsuarioId = GetUserId(claims);

            var success = await _artigoService.AtualizarMetadadosArtigoAsync(id, input, currentUsuarioId, commentary);

            if (success)
            {
                var updatedEntity = await _artigoService.ObterArtigoParaEditorialAsync(id, currentUsuarioId)
                                       ?? throw new InvalidOperationException("Artigo atualizado, mas falha ao recuperá-lo.");
                return _mapper.Map<ArtigoDTO>(updatedEntity);
            }
            throw new InvalidOperationException("Falha ao atualizar metadados do artigo.");
        }

        public async Task<bool> AtualizarConteudoArtigoAsync(
            string artigoId,
            string newContent,
            List<MidiaEntryInputDTO> midias,
            string commentary,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = GetUserId(claims);
            var midiasEntidades = _mapper.Map<List<MidiaEntry>>(midias);
            return await _artigoService.AtualizarConteudoArtigoAsync(artigoId, newContent, midiasEntidades, currentUsuarioId, commentary);
        }

        public async Task<Editorial> AtualizarEquipeEditorialAsync(
            string artigoId,
            EditorialTeam teamInput,
            string commentary,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = GetUserId(claims);
            return await _artigoService.AtualizarEquipeEditorialAsync(artigoId, teamInput, currentUsuarioId, commentary);
        }

        // =========================================================================
        // INTERACTION MUTATIONS
        // =========================================================================

        public async Task<Interaction> CreatePublicCommentAsync(
            string artigoId,
            string content,
            string usuarioNome,
            string? parentCommentId,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = GetUserId(claims);
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
            var currentUsuarioId = GetUserId(claims);
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

        public async Task<Interaction> AtualizarInteracaoAsync(
            string interacaoId,
            string newContent,
            string commentary,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = GetUserId(claims);
            return await _artigoService.AtualizarInteracaoAsync(interacaoId, newContent, currentUsuarioId, commentary);
        }

        public async Task<bool> DeletarInteracaoAsync(
            string interacaoId,
            string commentary,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = GetUserId(claims);
            return await _artigoService.DeletarInteracaoAsync(interacaoId, currentUsuarioId, commentary);
        }

        // =========================================================================
        // STAFF COMMENT MUTATIONS
        // =========================================================================

        public async Task<ArtigoHistory> AddStaffComentarioAsync(
            string historyId,
            string comment,
            string? parent,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = GetUserId(claims);
            return await _artigoService.AddStaffComentarioAsync(historyId, currentUsuarioId, comment, parent);
        }

        public async Task<ArtigoHistory> UpdateStaffComentarioAsync(
            string historyId,
            string comentarioId,
            string newContent,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = GetUserId(claims);
            return await _artigoService.UpdateStaffComentarioAsync(historyId, comentarioId, newContent, currentUsuarioId);
        }

        public async Task<ArtigoHistory> DeleteStaffComentarioAsync(
            string historyId,
            string comentarioId,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = GetUserId(claims);
            return await _artigoService.DeleteStaffComentarioAsync(historyId, comentarioId, currentUsuarioId);
        }


        // =========================================================================
        // STAFF MANAGEMENT MUTATIONS
        // =========================================================================

        public async Task<Staff> CriarNovoStaffAsync(
            CreateStaffRequest input,
            string commentary,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = GetUserId(claims);
            var novoStaff = _mapper.Map<Staff>(input);
            return await _artigoService.CriarNovoStaffAsync(novoStaff, currentUsuarioId, commentary);
        }

        public async Task<StaffViewDTO> AtualizarStaffAsync(
            UpdateStaffInput input,
            string commentary,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = GetUserId(claims);
            var updatedStaffEntity = await _artigoService.AtualizarStaffAsync(input, currentUsuarioId, commentary);
            return _mapper.Map<StaffViewDTO>(updatedStaffEntity);
        }

        // =========================================================================
        // VOLUME MANAGEMENT MUTATIONS
        // =========================================================================

        public async Task<Volume> CriarVolumeAsync(
            CreateVolumeRequest input,
            string commentary,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = GetUserId(claims);
            var novoVolume = _mapper.Map<Volume>(input);
            return await _artigoService.CriarVolumeAsync(novoVolume, currentUsuarioId, commentary);
        }

        public async Task<bool> AtualizarMetadadosVolumeAsync(
            string volumeId,
            UpdateVolumeMetadataInput input,
            string commentary,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = GetUserId(claims);
            return await _artigoService.AtualizarMetadadosVolumeAsync(volumeId, input, currentUsuarioId, commentary);
        }

        // =========================================================================
        // PENDING MANAGEMENT MUTATIONS
        // =========================================================================

        public async Task<Pending> CriarRequisicaoPendenteAsync(
            Pending input,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = GetUserId(claims);
            return await _artigoService.CriarRequisicaoPendenteAsync(input, currentUsuarioId);
        }

        public async Task<bool> ResolverRequisicaoPendenteAsync(
            string pendingId,
            bool isApproved,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = GetUserId(claims);
            return await _artigoService.ResolverRequisicaoPendenteAsync(pendingId, isApproved, currentUsuarioId);
        }
    }
}