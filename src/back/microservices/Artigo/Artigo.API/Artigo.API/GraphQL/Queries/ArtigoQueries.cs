using Artigo.Intf.Enums;
using Artigo.Intf.Interfaces;
using Artigo.Server.DTOs;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Artigo.API.GraphQL.Queries
{
    /// <summary>
    /// Define os métodos de consulta (Query) do GraphQL, utilizando Constructor Injection.
    /// Esta classe é mapeada manualmente em ArtigoQueryType.cs.
    /// </summary>
    public class ArtigoQueries
    {
        private readonly IArtigoService _artigoService;
        private readonly AutoMapper.IMapper _mapper;

        public ArtigoQueries(IArtigoService artigoService, AutoMapper.IMapper mapper)
        {
            _artigoService = artigoService;
            _mapper = mapper;
        }

        /// <sumario>
        /// NOVO: Consulta para obter todos os artigos publicados para o público geral (visitantes/leitores).
        /// Não requer autenticação.
        /// </sumario>
        public async Task<IReadOnlyList<ArtigoDTO>> ObterArtigosPublicadosParaVisitantesAsync()
        {
            var entities = await _artigoService.ObterArtigosPublicadosParaVisitantesAsync();
            return _mapper.Map<IReadOnlyList<ArtigoDTO>>(entities);
        }

        // RENOMEADO: GetArtigoByIdAsync -> ObterArtigoPorIdAsync
        public async Task<ArtigoDTO?> ObterArtigoPorIdAsync(
            string idArtigo, // Parâmetro renomeado para maior clareza em PT
            ClaimsPrincipal claims)
        {
            // Tenta obter o ID do usuário (pode ser string.Empty para não autenticados, mas necessário para AuthZ no service)
            var currentUsuarioId = claims.FindFirstValue("sub") ?? claims.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            // O serviço ObterArtigoParaEditorialAsync é usado para permitir acesso de staff
            var entity = await _artigoService.ObterArtigoParaEditorialAsync(idArtigo, currentUsuarioId);

            return _mapper.Map<ArtigoDTO>(entity);
        }

        // RENOMEADO: GetArtigosByStatusAsync -> ObterArtigosPorStatusAsync
        public async Task<IReadOnlyList<ArtigoDTO>> ObterArtigosPorStatusAsync(
            StatusArtigo status, // FIX: ArtigoStatus -> StatusArtigo
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? claims.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUsuarioId))
            {
                // Para consultas de status (exceto "Published"), o usuário deve ser Staff.
                // Se não autenticado, retorna lista vazia.
                return new List<ArtigoDTO>();
            }

            var entities = await _artigoService.ObterArtigosPorStatusAsync(status, currentUsuarioId);
            return _mapper.Map<IReadOnlyList<ArtigoDTO>>(entities);
        }
    }
}