using Artigo.Intf.Entities;
using Artigo.Intf.Interfaces;

namespace Artigo.API.GraphQL.DataLoaders
{
    public class AutorDataLoader : GroupedDataLoader<string, Autor>
    {
        private readonly IAutorRepository _repository;

        public AutorDataLoader(
            IBatchScheduler scheduler,
            IAutorRepository repository)
            : base(scheduler, new DataLoaderOptions())
        {
            _repository = repository;
        }

        protected override async Task<ILookup<string, Autor>> LoadGroupedBatchAsync(IReadOnlyList<string> keys, CancellationToken cancellationToken)
        {
            // keys é a lista consolidada de todos os AutorIds
            var autores = await _repository.GetByIdsAsync(keys, cancellationToken);

            // Retorna como ILookup, onde a chave é o Autor.Id
            return autores.ToLookup(a => a.Id, a => a);
        }
    }
}