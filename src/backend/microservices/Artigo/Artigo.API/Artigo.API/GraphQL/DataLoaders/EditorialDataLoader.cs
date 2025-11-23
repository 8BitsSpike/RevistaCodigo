using Artigo.Intf.Entities;
using Artigo.Intf.Interfaces;

namespace Artigo.API.GraphQL.DataLoaders
{
    public class EditorialDataLoader : BatchDataLoader<string, Editorial>
    {
        private readonly IEditorialRepository _repository;

        public EditorialDataLoader(
            IBatchScheduler scheduler,
            IEditorialRepository repository)
            : base(scheduler, new DataLoaderOptions())
        {
            _repository = repository;
        }

        protected override async Task<IReadOnlyDictionary<string, Editorial>> LoadBatchAsync(IReadOnlyList<string> keys, CancellationToken cancellationToken)
        {
            var editorials = await _repository.GetByIdsAsync(keys.ToList());
            return editorials.ToDictionary(e => e.Id);
        }
    }
}