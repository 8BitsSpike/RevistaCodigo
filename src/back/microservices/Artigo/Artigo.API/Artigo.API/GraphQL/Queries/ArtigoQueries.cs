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
    /// <summary>
    /// Define os métodos de consulta (Query) do GraphQL, utilizing Constructor Injection.
    /// Esta classe é mapeada manually em ArtigoQueryType.cs para evitar erros.
    /// </summary>
    public class ArtigoQueries
    {
        private readonly IArtigoService _artigoService;
        private readonly AutoMapper.IMapper _mapper;

        // Construtor atualizado (sem dependências de repositório)
        public ArtigoQueries(
            IArtigoService artigoService,
            AutoMapper.IMapper mapper)
        {
            _artigoService = artigoService;
            _mapper = mapper;
        }

        // =========================================================================
        // *** QUERIES PÚBLICAS (Formatos de Visitante) ***
        // =========================================================================

        /// <sumario>
        /// Consulta para obter artigos publicados no 'Card List Format'.
        /// Não requer autenticação.
        /// </sumario>
        public async Task<IReadOnlyList<ArtigoCardListDTO>> ObterArtigosCardListAsync(
            int pagina,
            int tamanho)
        {
            var entities = await _artigoService.ObterArtigosCardListAsync(pagina, tamanho);
            return _mapper.Map<IReadOnlyList<ArtigoCardListDTO>>(entities);
        }

        /// <sumario>
        /// Consulta para obter artigos publicados no 'Card List Format' filtrados por tipo.
        /// Não requer autenticação.
        /// </sumario>
        public async Task<IReadOnlyList<ArtigoCardListDTO>> ObterArtigosCardListPorTipoAsync(
            TipoArtigo tipo,
            int pagina,
            int tamanho)
        {
            var entities = await _artigoService.ObterArtigosCardListPorTipoAsync(tipo, pagina, tamanho);
            return _mapper.Map<IReadOnlyList<ArtigoCardListDTO>>(entities);
        }

        /// <sumario>
        /// Busca artigos (formato card) pelo título.
        /// </sumario>
        public async Task<IReadOnlyList<ArtigoCardListDTO>> ObterArtigosCardListPorTituloAsync(
            string searchTerm,
            int pagina,
            int tamanho)
        {
            var entities = await _artigoService.ObterArtigosCardListPorTituloAsync(searchTerm, pagina, tamanho);
            return _mapper.Map<IReadOnlyList<ArtigoCardListDTO>>(entities);
        }

        /// <sumario>
        /// Busca artigos (formato card) pelo nome do autor (registrado ou não).
        /// </sumario>
        public async Task<IReadOnlyList<ArtigoCardListDTO>> ObterArtigosCardListPorNomeAutorAsync(
            string searchTerm,
            int pagina,
            int tamanho)
        {
            var entities = await _artigoService.ObterArtigosCardListPorNomeAutorAsync(searchTerm, pagina, tamanho);
            // O serviço já retorna entidades de Artigo, prontas para serem mapeadas para o DTO de card.
            return _mapper.Map<IReadOnlyList<ArtigoCardListDTO>>(entities);
        }

        /// <sumario>
        /// Consulta para 'Card List Format' por Lista de IDs (Público)
        /// </sumario>
        public async Task<IReadOnlyList<ArtigoCardListDTO>> ObterArtigosCardListPorListaAsync(
            string[] ids)
        {
            // Converte array para IReadOnlyList
            var idList = ids.ToList().AsReadOnly();
            var entities = await _artigoService.ObterArtigosPorListaIdsAsync(idList);
            return _mapper.Map<IReadOnlyList<ArtigoCardListDTO>>(entities);
        }

        /// <sumario>
        /// Consulta para obter volumes publicados no 'Volume Card Format'.
        /// Não requer autenticação.
        /// </sumario>
        public async Task<IReadOnlyList<VolumeCardDTO>> ObterVolumesListAsync( // <-- TIPO DE RETORNO ALTERADO
            int pagina,
            int tamanho)
        {
            var entities = await _artigoService.ObterVolumesListAsync(pagina, tamanho);
            return _mapper.Map<IReadOnlyList<VolumeCardDTO>>(entities); // <-- MAPEAMENTO ALTERADO
        }

        /// <sumario>
        /// Consulta para obter um autor no 'Autor Format'.
        /// Não requer autenticação.
        /// </sumario>
        public async Task<AutorViewDTO?> ObterAutorViewAsync(string autorId)
        {
            var entity = await _artigoService.ObterAutorCardAsync(autorId);
            return _mapper.Map<AutorViewDTO>(entity);
        }

        /// <sumario>
        /// Consulta para obter um artigo publicado no 'Artigo Format'.
        /// Não requer autenticação.
        /// (N+1): Lógica de agregação removida.
        /// </sumario>
        public async Task<ArtigoViewDTO?> ObterArtigoViewAsync(string artigoId)
        {
            var artigo = await _artigoService.ObterArtigoViewAsync(artigoId);
            if (artigo == null)
            {
                return null;
            }
            // Mapeamento simples. Resolvers cuidam do resto.
            return _mapper.Map<ArtigoViewDTO>(artigo);
        }

        /// <sumario>
        /// Consulta para obter um volume publicado no 'Volume View Format'.
        /// Não requer autenticação.
        /// </sumario>
        public async Task<VolumeViewDTO?> ObterVolumeViewAsync(string volumeId)
        {
            // O serviço ObterVolumeViewAsync já contém a lógica de AuthZ (só retorna se for 'Publicado')
            var entity = await _artigoService.ObterVolumeViewAsync(volumeId);
            if (entity == null)
            {
                return null;
            }
            return _mapper.Map<VolumeViewDTO>(entity);
        }

        /// <sumario>
        /// Consulta para obter um autor no 'Autor Card Format'.
        /// Não requer autenticação.
        /// </sumario>
        public async Task<AutorCardDTO?> ObterAutorCardAsync(string autorId)
        {
            var entity = await _artigoService.ObterAutorCardAsync(autorId);
            return _mapper.Map<AutorCardDTO>(entity);
        }

        /// <sumario>
        /// Consulta para obter um volume no 'Volume Card Format'.
        /// Não requer autenticação.
        /// </sumario>
        public async Task<VolumeCardDTO?> ObterVolumeCardAsync(string volumeId)
        {
            var entity = await _artigoService.ObterVolumeCardAsync(volumeId);
            return _mapper.Map<VolumeCardDTO>(entity);
        }

        public async Task<IReadOnlyList<Interaction>> ObterComentariosPublicosAsync(
            string artigoId,
            int pagina,
            int tamanho)
        {
            // O serviço não mapeia, ele retorna a entidade diretamente.
            // O GraphQL <InteractionType> lidará com a entidade.
            return await _artigoService.ObterComentariosPublicosAsync(artigoId, pagina, tamanho);
        }


        // =========================================================================
        // *** QUERIES INTERNAS (Staff/Usuários Autenticados) ***
        // =========================================================================

        /// <sumario>
        /// Consulta para obter um artigo no 'Artigo Editorial Format'.
        /// Requer autenticação (Autor ou Staff).
        /// </sumario>
        public async Task<ArtigoEditorialViewDTO?> ObterArtigoEditorialViewAsync(
            string artigoId,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? claims.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("Usuário deve estar autenticado para esta visualização.");

            // O serviço ObterArtigoEditorialViewAsync já contém a lógica de AuthZ
            var artigo = await _artigoService.ObterArtigoEditorialViewAsync(artigoId, currentUsuarioId);
            if (artigo == null)
            {
                return null;
            }
            // Mapeamento simples. Resolvers cuidam do resto.
            return _mapper.Map<ArtigoEditorialViewDTO>(artigo);
        }

        /// <sumario>
        /// Consulta para obter todos os artigos publicados para usuários não cadastrados (visitantes/leitores), com paginação.
        /// Não requer autenticação.
        /// </sumario>
        public async Task<IReadOnlyList<ArtigoDTO>> ObterArtigosPublicadosParaVisitantesAsync(
            int pagina,
            int tamanho)
        {
            var entities = await _artigoService.ObterArtigosPublicadosParaVisitantesAsync(pagina, tamanho);
            return _mapper.Map<IReadOnlyList<ArtigoDTO>>(entities);
        }

        public async Task<ArtigoDTO?> ObterArtigoPorIdAsync(
            string idArtigo,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? claims.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var entity = await _artigoService.ObterArtigoParaEditorialAsync(idArtigo, currentUsuarioId);
            return _mapper.Map<ArtigoDTO>(entity);
        }

        /// <sumario>
        /// Consulta para obter artigos por status no 'Card List Format'.
        /// </sumario>
        public async Task<IReadOnlyList<ArtigoCardListDTO>> ObterArtigosPorStatusAsync(
            StatusArtigo status,
            int pagina,
            int tamanho,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? claims.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUsuarioId))
            {
                return new List<ArtigoCardListDTO>();
            }

            var entities = await _artigoService.ObterArtigosPorStatusAsync(status, pagina, tamanho, currentUsuarioId);
            return _mapper.Map<IReadOnlyList<ArtigoCardListDTO>>(entities);
        }

        /// <sumario>
        /// (NOVO) Consulta para obter os artigos de um autor (formato card).
        /// Requer autenticação.
        /// </sumario>
        public async Task<IReadOnlyList<ArtigoCardListDTO>> ObterMeusArtigosCardListAsync(
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? claims.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("Usuário deve estar autenticado para esta visualização.");

            var entities = await _artigoService.ObterMeusArtigosCardListAsync(currentUsuarioId);
            // Mapeia o resultado (que já está no formato de card, mas como entidade) para o DTO
            return _mapper.Map<IReadOnlyList<ArtigoCardListDTO>>(entities);
        }

        /// <sumario>
        /// Consulta para obter todos os pedidos pendentes, com filtros opcionais.
        /// REGRA: Requer AuthZ (EditorChefe ou Administrador) para visualização.
        /// </sumario>
        public async Task<IReadOnlyList<Pending>> ObterPendentesAsync(
            int pagina,
            int tamanho,
            ClaimsPrincipal claims,
            // Argumentos de filtro opcionais
            StatusPendente? status,
            string? targetEntityId,
            TipoEntidadeAlvo? targetType,
            string? requesterUsuarioId)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? claims.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            if (string.IsNullOrEmpty(currentUsuarioId))
            {
                return new List<Pending>();
            }

            // Lógica de roteamento de filtro:
            if (status.HasValue)
            {
                return await _artigoService.ObterPendentesPorStatusAsync(status.Value, pagina, tamanho, currentUsuarioId);
            }

            if (!string.IsNullOrEmpty(targetEntityId))
            {
                return await _artigoService.ObterPendenciasPorEntidadeIdAsync(targetEntityId, currentUsuarioId);
            }

            if (targetType.HasValue)
            {
                return await _artigoService.ObterPendenciasPorTipoDeEntidadeAsync(targetType.Value, currentUsuarioId);
            }

            if (!string.IsNullOrEmpty(requesterUsuarioId))
            {
                return await _artigoService.ObterPendenciasPorRequisitanteIdAsync(requesterUsuarioId, currentUsuarioId);
            }

            // Padrão: Retorna todos os pendentes, paginados
            return await _artigoService.ObterPendentesAsync(pagina, tamanho, currentUsuarioId);
        }

        /// <sumario>
        /// Consulta para obter todos os registros de Autor no sistema, com paginação.
        /// REGRA: Requer permissão de Staff.
        /// </sumario>
        public async Task<IReadOnlyList<Autor>> ObterAutoresAsync(
            int pagina,
            int tamanho,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? claims.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            if (string.IsNullOrEmpty(currentUsuarioId))
            {
                throw new UnauthorizedAccessException("Usuário deve estar autenticado para listar todos os autores.");
            }

            return await _artigoService.ObterAutoresAsync(pagina, tamanho, currentUsuarioId);
        }

        /// <sumario>
        /// Consulta para obter um registro de Autor específico por ID.
        /// REGRA: Requer permissão de Staff.
        /// </sumario>
        public async Task<Autor?> ObterAutorPorIdAsync(
            string idAutor,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? claims.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            if (string.IsNullOrEmpty(currentUsuarioId))
            {
                throw new UnauthorizedAccessException("Usuário deve estar autenticado para buscar um autor.");
            }

            return await _artigoService.ObterAutorPorIdAsync(idAutor, currentUsuarioId);
        }

        public async Task<IReadOnlyList<VolumeCardDTO>> ObterVolumesAsync( // <-- TIPO DE RETORNO ALTERADO
            int pagina,
            int tamanho,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? claims.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            if (string.IsNullOrEmpty(currentUsuarioId))
            {
                throw new UnauthorizedAccessException("Usuário deve estar autenticado para listar volumes.");
            }

            var entities = await _artigoService.ObterVolumesAsync(pagina, tamanho, currentUsuarioId);
            return _mapper.Map<IReadOnlyList<VolumeCardDTO>>(entities); // <-- MAPEAMENTO ALTERADO
        }

        public async Task<IReadOnlyList<Volume>> ObterVolumesPorAnoAsync(
            int ano,
            int pagina,
            int tamanho,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? claims.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            if (string.IsNullOrEmpty(currentUsuarioId))
            {
                throw new UnauthorizedAccessException("Usuário deve estar autenticado para listar volumes por ano.");
            }

            return await _artigoService.ObterVolumesPorAnoAsync(ano, pagina, tamanho, currentUsuarioId);
        }

        public async Task<IReadOnlyList<VolumeCardDTO>> ObterVolumesPorStatusAsync(
            StatusVolume status,
            int pagina,
            int tamanho,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? claims.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            var entities = await _artigoService.ObterVolumesPorStatusAsync(status, pagina, tamanho, currentUsuarioId);
            return _mapper.Map<IReadOnlyList<VolumeCardDTO>>(entities);
        }

        public async Task<Volume?> ObterVolumePorIdAsync(
            string idVolume,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? claims.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            if (string.IsNullOrEmpty(currentUsuarioId))
            {
                throw new UnauthorizedAccessException("Usuário deve estar autenticado para buscar um volume.");
            }

            return await _artigoService.ObterVolumePorIdAsync(idVolume, currentUsuarioId);
        }

        public async Task<StaffViewDTO?> ObterStaffPorIdAsync(
            string staffId,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? claims.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            var entity = await _artigoService.ObterStaffPorIdAsync(staffId, currentUsuarioId);
            return _mapper.Map<StaffViewDTO>(entity);
        }

        public async Task<IReadOnlyList<StaffViewDTO>> ObterStaffListAsync(
            int pagina,
            int tamanho,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? claims.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("Usuário deve estar autenticado.");

            var entities = await _artigoService.ObterStaffListAsync(pagina, tamanho, currentUsuarioId);
            return _mapper.Map<IReadOnlyList<StaffViewDTO>>(entities);
        }
    }
}