using Artigo.Intf.Entities;
using Artigo.Intf.Interfaces;

namespace Artigo.API.GraphQL.DataLoaders
{
    public class InteractionRepliesDataLoader : GroupedDataLoader<string, Interaction>
    {
        private readonly IInteractionRepository _repository;

        public InteractionRepliesDataLoader(
            IBatchScheduler scheduler,
            IInteractionRepository repository)
            : base(scheduler, new DataLoaderOptions())
        {
            _repository = repository;
        }

        protected override async Task<ILookup<string, Interaction>> LoadGroupedBatchAsync(IReadOnlyList<string> keys, CancellationToken cancellationToken)
        {
            // keys é a lista consolidada de IDs dos comentários "pai"
            var replies = await _repository.GetByParentIdsAsync(keys);

            // Agrupa as respostas usando o ParentCommentId como chave, conforme o contrato proposto para o GroupedDataLoader.
            return replies.ToLookup(i => i.ParentCommentId ?? string.Empty, i => i);
        }
    }
}