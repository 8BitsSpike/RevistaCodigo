using Artigo.Intf.Enums;
using Artigo.Intf.Interfaces;
using Artigo.Server.DTOs;
using Artigo.Intf.Entities;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using System.Linq;


namespace Artigo.API.GraphQL.Queries
{
    public class ArtigoQueries
    {
        private readonly IArtigoService _artigoService;
        private readonly AutoMapper.IMapper _mapper;

        public ArtigoQueries(IArtigoService artigoService, AutoMapper.IMapper mapper)
        {
            _artigoService = artigoService;
            _mapper = mapper;
        }

        // Queries Públicas (Mantidas)
        public async Task<IReadOnlyList<ArtigoCardListDTO>> ObterArtigosCardListAsync(int pagina, int tamanho)
        {
            var entities = await _artigoService.ObterArtigosCardListAsync(pagina, tamanho);
            return _mapper.Map<IReadOnlyList<ArtigoCardListDTO>>(entities);
        }

        public async Task<IReadOnlyList<ArtigoCardListDTO>> ObterArtigosCardListPorTipoAsync(TipoArtigo tipo, int pagina, int tamanho)
        {
            var entities = await _artigoService.ObterArtigosCardListPorTipoAsync(tipo, pagina, tamanho);
            return _mapper.Map<IReadOnlyList<ArtigoCardListDTO>>(entities);
        }

        public async Task<IReadOnlyList<ArtigoCardListDTO>> ObterArtigosCardListPorTituloAsync(string searchTerm, int pagina, int tamanho)
        {
            var entities = await _artigoService.ObterArtigosCardListPorTituloAsync(searchTerm, pagina, tamanho);
            return _mapper.Map<IReadOnlyList<ArtigoCardListDTO>>(entities);
        }

        public async Task<IReadOnlyList<ArtigoCardListDTO>> ObterArtigosCardListPorNomeAutorAsync(string searchTerm, int pagina, int tamanho)
        {
            var entities = await _artigoService.ObterArtigosCardListPorNomeAutorAsync(searchTerm, pagina, tamanho);
            return _mapper.Map<IReadOnlyList<ArtigoCardListDTO>>(entities);
        }

        public async Task<IReadOnlyList<ArtigoCardListDTO>> ObterArtigosCardListPorListaAsync(string[] ids)
        {
            var idList = ids.ToList().AsReadOnly();
            var entities = await _artigoService.ObterArtigosPorListaIdsAsync(idList);
            return _mapper.Map<IReadOnlyList<ArtigoCardListDTO>>(entities);
        }

        public async Task<IReadOnlyList<VolumeCardDTO>> ObterVolumesListAsync(int pagina, int tamanho)
        {
            var entities = await _artigoService.ObterVolumesListAsync(pagina, tamanho);
            return _mapper.Map<IReadOnlyList<VolumeCardDTO>>(entities);
        }

        public async Task<AutorViewDTO?> ObterAutorViewAsync(string autorId)
        {
            var entity = await _artigoService.ObterAutorCardAsync(autorId);
            return _mapper.Map<AutorViewDTO>(entity);
        }

        public async Task<ArtigoViewDTO?> ObterArtigoViewAsync(string artigoId)
        {
            var artigo = await _artigoService.ObterArtigoViewAsync(artigoId);
            if (artigo == null) return null;
            return _mapper.Map<ArtigoViewDTO>(artigo);
        }

        public async Task<VolumeViewDTO?> ObterVolumeViewAsync(string volumeId)
        {
            var entity = await _artigoService.ObterVolumeViewAsync(volumeId);
            if (entity == null) return null;
            return _mapper.Map<VolumeViewDTO>(entity);
        }

        public async Task<AutorCardDTO?> ObterAutorCardAsync(string autorId)
        {
            var entity = await _artigoService.ObterAutorCardAsync(autorId);
            return _mapper.Map<AutorCardDTO>(entity);
        }

        public async Task<VolumeCardDTO?> ObterVolumeCardAsync(string volumeId)
        {
            var entity = await _artigoService.ObterVolumeCardAsync(volumeId);
            return _mapper.Map<VolumeCardDTO>(entity);
        }

        public async Task<IReadOnlyList<Interaction>> ObterComentariosPublicosAsync(string artigoId, int pagina, int tamanho)
        {
            return await _artigoService.ObterComentariosPublicosAsync(artigoId, pagina, tamanho);
        }

        // --------------------------------------------------------------------------------
        // Queries Internas (Now Imperative Security)
        // --------------------------------------------------------------------------------

        public async Task<ArtigoEditorialViewDTO?> ObterArtigoEditorialViewAsync(string artigoId, ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? string.Empty;
            if (string.IsNullOrEmpty(currentUsuarioId)) throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            var artigo = await _artigoService.ObterArtigoEditorialViewAsync(artigoId, currentUsuarioId);
            if (artigo == null) return null;
            return _mapper.Map<ArtigoEditorialViewDTO>(artigo);
        }

        public async Task<bool> VerificarStaffAsync(ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? claims.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUsuarioId)) return false;
            return await _artigoService.VerificarStaffAsync(currentUsuarioId);
        }

        public async Task<IReadOnlyList<ArtigoDTO>> ObterArtigosPublicadosParaVisitantesAsync(int pagina, int tamanho)
        {
            // Nota: Este método é público no serviço, mas está agrupado com staff/auth queries no GraphQL.
            var entities = await _artigoService.ObterArtigosPublicadosParaVisitantesAsync(pagina, tamanho);
            return _mapper.Map<IReadOnlyList<ArtigoDTO>>(entities);
        }

        public async Task<ArtigoDTO?> ObterArtigoPorIdAsync(string idArtigo, ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? string.Empty;
            if (string.IsNullOrEmpty(currentUsuarioId)) throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            var entity = await _artigoService.ObterArtigoParaEditorialAsync(idArtigo, currentUsuarioId);
            return _mapper.Map<ArtigoDTO>(entity);
        }

        public async Task<IReadOnlyList<ArtigoCardListDTO>> ObterArtigosPorStatusAsync(StatusArtigo status, int pagina, int tamanho, ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? string.Empty;
            if (string.IsNullOrEmpty(currentUsuarioId)) throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            var entities = await _artigoService.ObterArtigosPorStatusAsync(status, pagina, tamanho, currentUsuarioId);
            return _mapper.Map<IReadOnlyList<ArtigoCardListDTO>>(entities);
        }

        public async Task<IReadOnlyList<ArtigoCardListDTO>> ObterMeusArtigosCardListAsync(ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? string.Empty;
            if (string.IsNullOrEmpty(currentUsuarioId)) throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            var entities = await _artigoService.ObterMeusArtigosCardListAsync(currentUsuarioId);
            return _mapper.Map<IReadOnlyList<ArtigoCardListDTO>>(entities);
        }

        public async Task<IReadOnlyList<Pending>> ObterPendentesAsync(
            int pagina, int tamanho,
            StatusPendente? status, string? targetEntityId, TipoEntidadeAlvo? targetType, string? requesterUsuarioId,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? string.Empty;
            if (string.IsNullOrEmpty(currentUsuarioId)) throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            if (status.HasValue) return await _artigoService.ObterPendentesPorStatusAsync(status.Value, pagina, tamanho, currentUsuarioId);
            if (!string.IsNullOrEmpty(targetEntityId)) return await _artigoService.ObterPendenciasPorEntidadeIdAsync(targetEntityId, currentUsuarioId);
            if (targetType.HasValue) return await _artigoService.ObterPendenciasPorTipoDeEntidadeAsync(targetType.Value, currentUsuarioId);
            if (!string.IsNullOrEmpty(requesterUsuarioId)) return await _artigoService.ObterPendenciasPorRequisitanteIdAsync(requesterUsuarioId, currentUsuarioId);
            return await _artigoService.ObterPendentesAsync(pagina, tamanho, currentUsuarioId);
        }

        public async Task<IReadOnlyList<Autor>> ObterAutoresAsync(int pagina, int tamanho, ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? string.Empty;
            if (string.IsNullOrEmpty(currentUsuarioId)) throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            return await _artigoService.ObterAutoresAsync(pagina, tamanho, currentUsuarioId);
        }

        public async Task<Autor?> ObterAutorPorIdAsync(string idAutor, ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? string.Empty;
            if (string.IsNullOrEmpty(currentUsuarioId)) throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            return await _artigoService.ObterAutorPorIdAsync(idAutor, currentUsuarioId);
        }

        public async Task<IReadOnlyList<VolumeCardDTO>> ObterVolumesAsync(int pagina, int tamanho, ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? string.Empty;
            if (string.IsNullOrEmpty(currentUsuarioId)) throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            var entities = await _artigoService.ObterVolumesAsync(pagina, tamanho, currentUsuarioId);
            return _mapper.Map<IReadOnlyList<VolumeCardDTO>>(entities);
        }

        public async Task<IReadOnlyList<Volume>> ObterVolumesPorAnoAsync(int ano, int pagina, int tamanho, ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? string.Empty;
            if (string.IsNullOrEmpty(currentUsuarioId)) throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            return await _artigoService.ObterVolumesPorAnoAsync(ano, pagina, tamanho, currentUsuarioId);
        }

        public async Task<IReadOnlyList<VolumeCardDTO>> ObterVolumesPorStatusAsync(StatusVolume status, int pagina, int tamanho, ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? string.Empty;
            if (string.IsNullOrEmpty(currentUsuarioId)) throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            var entities = await _artigoService.ObterVolumesPorStatusAsync(status, pagina, tamanho, currentUsuarioId);
            return _mapper.Map<IReadOnlyList<VolumeCardDTO>>(entities);
        }

        public async Task<Volume?> ObterVolumePorIdAsync(string idVolume, ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? string.Empty;
            if (string.IsNullOrEmpty(currentUsuarioId)) throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            return await _artigoService.ObterVolumePorIdAsync(idVolume, currentUsuarioId);
        }

        public async Task<StaffViewDTO?> ObterStaffPorIdAsync(string staffId, ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? string.Empty;
            if (string.IsNullOrEmpty(currentUsuarioId)) throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            var entity = await _artigoService.ObterStaffPorIdAsync(staffId, currentUsuarioId);
            return _mapper.Map<StaffViewDTO>(entity);
        }

        public async Task<IReadOnlyList<StaffViewDTO>> ObterStaffListAsync(int pagina, int tamanho, ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? string.Empty;
            if (string.IsNullOrEmpty(currentUsuarioId)) throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            var entities = await _artigoService.ObterStaffListAsync(pagina, tamanho, currentUsuarioId);
            return _mapper.Map<IReadOnlyList<StaffViewDTO>>(entities);
        }

        public async Task<IReadOnlyList<ArtigoCardListDTO>> ObterArtigosEditorialPorTipoAsync(TipoArtigo tipo, int pagina, int tamanho, ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? string.Empty;
            if (string.IsNullOrEmpty(currentUsuarioId)) throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            var entities = await _artigoService.ObterArtigosEditorialPorTipoAsync(tipo, pagina, tamanho, currentUsuarioId);
            return _mapper.Map<IReadOnlyList<ArtigoCardListDTO>>(entities);
        }

        public async Task<IReadOnlyList<ArtigoCardListDTO>> SearchArtigosEditorialByTitleAsync(string searchTerm, int pagina, int tamanho, ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? string.Empty;
            if (string.IsNullOrEmpty(currentUsuarioId)) throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            var entities = await _artigoService.SearchArtigosEditorialByTitleAsync(searchTerm, pagina, tamanho, currentUsuarioId);
            return _mapper.Map<IReadOnlyList<ArtigoCardListDTO>>(entities);
        }

        public async Task<IReadOnlyList<ArtigoCardListDTO>> SearchArtigosEditorialByAutorIdsAsync(string[] idsAutor, int pagina, int tamanho, ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? string.Empty;
            if (string.IsNullOrEmpty(currentUsuarioId)) throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            var idList = idsAutor.ToList().AsReadOnly();
            var entities = await _artigoService.SearchArtigosEditorialByAutorIdsAsync(idList, pagina, tamanho, currentUsuarioId);
            return _mapper.Map<IReadOnlyList<ArtigoCardListDTO>>(entities);
        }
    }
}