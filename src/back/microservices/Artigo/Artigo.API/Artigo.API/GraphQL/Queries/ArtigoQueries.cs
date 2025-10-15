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

        public async Task<ArtigoDTO?> GetArtigoByIdAsync(
            string id,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? claims.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var entity = await _artigoService.GetArtigoForEditorialAsync(id, currentUsuarioId);
            return _mapper.Map<ArtigoDTO>(entity);
        }

        public async Task<IReadOnlyList<ArtigoDTO>> GetArtigosByStatusAsync(
            ArtigoStatus status,
            ClaimsPrincipal claims)
        {
            var currentUsuarioId = claims.FindFirstValue("sub") ?? claims.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUsuarioId))
            {
                return new List<ArtigoDTO>();
            }

            var entities = await _artigoService.GetArtigosByStatusAsync(status, currentUsuarioId);
            return _mapper.Map<IReadOnlyList<ArtigoDTO>>(entities);
        }
    }
}
