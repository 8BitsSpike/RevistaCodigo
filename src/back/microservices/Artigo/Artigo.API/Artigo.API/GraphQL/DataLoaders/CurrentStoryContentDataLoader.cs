using Artigo.Intf.Interfaces;

namespace Artigo.API.GraphQL.DataLoaders
{
    /// <sumario>
    /// DataLoader para buscar o CONTEÚDO atual de um artigo (string), 
    /// usando o EditorialId como chave.
    /// Requer buscar o Editorial e, em seguida, o ArtigoHistory referenciado.
    /// </sumario>
    public class CurrentHistoryContentDataLoader : BatchDataLoader<string, string>
    {
        private readonly IEditorialRepository _editorialRepository;
        private readonly IArtigoHistoryRepository _historyRepository;

        public CurrentHistoryContentDataLoader(
            IBatchScheduler scheduler,
            IEditorialRepository editorialRepository,
            IArtigoHistoryRepository historyRepository)
            : base(scheduler, new DataLoaderOptions()) // Usando o FIX de DataLoaderOptions
        {
            _editorialRepository = editorialRepository;
            _historyRepository = historyRepository;
        }

        protected override async Task<IReadOnlyDictionary<string, string>> LoadBatchAsync(IReadOnlyList<string> keys, CancellationToken cancellationToken)
        {
            // keys = Lista de EditorialIds
            var editorials = await _editorialRepository.GetByIdsAsync(keys.ToList());

            // 1. Extrai todos os CurrentHistoryIds necessários
            var historyIds = editorials.Select(e => e.CurrentHistoryId).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();

            // 2. Busca todos os ArtigoHistory em lote
            var histories = await _historyRepository.GetByIdsAsync(historyIds);
            var historyLookup = histories.ToDictionary(h => h.Id, h => h.Content);

            var result = new Dictionary<string, string>();

            foreach (var editorial in editorials)
            {
                // 3. Mapeia o conteúdo de volta para o EditorialId original (a chave)
                string content = string.Empty;
                if (!string.IsNullOrEmpty(editorial.CurrentHistoryId) && historyLookup.TryGetValue(editorial.CurrentHistoryId, out var hContent))
                {
                    content = hContent;
                }
                result[editorial.Id] = content;
            }

            return result;
        }
    }
}