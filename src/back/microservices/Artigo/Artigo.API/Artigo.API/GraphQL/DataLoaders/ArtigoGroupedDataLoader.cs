using Artigo.Intf.Interfaces;
using Artigo.Server.DTOs;
using AutoMapper;

namespace Artigo.API.GraphQL.DataLoaders
{
    public class ArtigoGroupedDataLoader : GroupedDataLoader<string, ArtigoDTO>
    {
        private readonly IArtigoRepository _artigoRepository;
        private readonly IMapper _mapper;

        public ArtigoGroupedDataLoader(
            IBatchScheduler scheduler,
            IArtigoRepository artigoRepository,
            IMapper mapper)
            : base(scheduler, new DataLoaderOptions())
        {
            _artigoRepository = artigoRepository;
            _mapper = mapper;
        }

        protected override async Task<ILookup<string, ArtigoDTO>> LoadGroupedBatchAsync(IReadOnlyList<string> keys, CancellationToken cancellationToken)
        {
            // 1. Busca as Entidades do Domínio em lote
            var artigos = await _artigoRepository.GetByIdsAsync(keys.ToList());

            // 2. Mapeia as Entidades (Artigo) para DTOs (ArtigoDTO)
            var dtos = _mapper.Map<IReadOnlyList<ArtigoDTO>>(artigos);

            // 3. Retorna como ILookup, usando o ID do Artigo como chave de agrupamento
            return dtos.ToLookup(a => a.Id, a => a);
        }
    }
}