using Artigo.Intf.Interfaces;
using Artigo.Server.DTOs;
using GreenDonut;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

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

        protected override async Task<ILookup<string, ArtigoDTO>> LoadGroupedBatchAsync(
            IReadOnlyList<string> keys,
            CancellationToken cancellationToken)
        {
            var artigos = await _artigoRepository.GetByIdsAsync(keys.ToList());
            var dtos = _mapper.Map<IReadOnlyList<ArtigoDTO>>(artigos);
            return dtos.ToLookup(a => a.Id, a => a);
        }
    }
}