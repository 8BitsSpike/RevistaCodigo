using Artigo.Intf.Entities;
using Artigo.Intf.Interfaces;
using GreenDonut; // Necessário para IBatchScheduler
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
            : base(scheduler, new DataLoaderOptions())
        {
            _editorialRepository = editorialRepository;
            _historyRepository = historyRepository;
        }

        protected override async Task<IReadOnlyDictionary<string, string>> LoadBatchAsync(
            IReadOnlyList<string> keys, // keys = Lista de EditorialIds
            CancellationToken cancellationToken)
        {
            // 1. Busca todos os Editoriais em lote
            var editorials = await _editorialRepository.GetByIdsAsync(keys.ToList());

            // 2. Extrai todos os CurrentHistoryIds necessários
            var historyIds = editorials
                .Select(e => e.CurrentHistoryId)
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();

            // 3. Busca todos os ArtigoHistory.Content em um único lote
            var historyContents = await _historyRepository.GetContentsByIdsAsync(historyIds);

            var result = new Dictionary<string, string>();

            foreach (var editorial in editorials)
            {
                // 4. Mapeia o conteúdo de volta para o EditorialId original (a chave)
                string content = string.Empty;
                if (!string.IsNullOrEmpty(editorial.CurrentHistoryId) &&
                    historyContents.TryGetValue(editorial.CurrentHistoryId, out var hContent))
                {
                    content = hContent;
                }

                // Garante que cada chave (EditorialId) tenha uma entrada no dicionário
                if (!result.ContainsKey(editorial.Id))
                {
                    result[editorial.Id] = content;
                }
            }

            // Garante que qualquer chave solicitada que não tenha um editorial correspondente
            // ainda receba uma string vazia.
            foreach (var key in keys)
            {
                if (!result.ContainsKey(key))
                {
                    result[key] = string.Empty;
                }
            }

            return result;
        }
    }
}