using Artigo.Intf.Entities;
using Artigo.Intf.Interfaces;

namespace Artigo.API.GraphQL.DataLoaders
{
    public class InteractionDataLoader : BatchDataLoader<string, Interaction>
    {
        private readonly IInteractionRepository _repository;

        public InteractionDataLoader(
            IBatchScheduler scheduler,
            IInteractionRepository repository)
            : base(scheduler, new DataLoaderOptions())
        {
            _repository = repository;
        }

        protected override async Task<IReadOnlyDictionary<string, Interaction>> LoadBatchAsync(IReadOnlyList<string> keys, CancellationToken cancellationToken)
        {
            var interactions = await _repository.GetByIdsAsync(keys.ToList());
            return interactions.ToDictionary(i => i.Id);
        }
    }
}