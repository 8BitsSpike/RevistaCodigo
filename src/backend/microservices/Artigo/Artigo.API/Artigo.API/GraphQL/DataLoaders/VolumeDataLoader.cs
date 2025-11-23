using Artigo.Intf.Entities;
using Artigo.Intf.Interfaces;

namespace Artigo.API.GraphQL.DataLoaders
{
    public class VolumeDataLoader : BatchDataLoader<string, Volume>
    {
        private readonly IVolumeRepository _repository;

        public VolumeDataLoader(
            IBatchScheduler scheduler,
            IVolumeRepository repository)
            : base(scheduler, new DataLoaderOptions())
        {
            _repository = repository;
        }

        protected override async Task<IReadOnlyDictionary<string, Volume>> LoadBatchAsync(IReadOnlyList<string> keys, CancellationToken cancellationToken)
        {
            var volumes = await _repository.GetByIdsAsync(keys.ToList());
            return volumes.ToDictionary(v => v.Id);
        }
    }
}