using Artigo.Intf.Entities;
using Artigo.Intf.Interfaces;

namespace Artigo.API.GraphQL.DataLoaders
{
    public class ArtigoHistoryGroupedDataLoader : GroupedDataLoader<string, ArtigoHistory>
    {
        private readonly IArtigoHistoryRepository _repository;

        public ArtigoHistoryGroupedDataLoader(
            IBatchScheduler scheduler,
            IArtigoHistoryRepository repository)
            : base(scheduler, new DataLoaderOptions())
        {
            _repository = repository;
        }

        protected override async Task<ILookup<string, ArtigoHistory>> LoadGroupedBatchAsync(IReadOnlyList<string> keys, CancellationToken cancellationToken)
        {
            // keys é a lista consolidada de todos os HistoryIds
            var historyEntries = await _repository.GetByIdsAsync(keys.ToList());

            // Retorna como ILookup, onde a chave é o ArtigoHistory.Id
            return historyEntries.ToLookup(h => h.Id, h => h);
        }
    }
}