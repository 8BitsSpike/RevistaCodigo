using Artigo.Intf.Entities;
using Artigo.Intf.Interfaces;
using GreenDonut; // Presente para garantir que vai ser usado o GreenDonut na aplicação.

namespace Artigo.API.GraphQL.DataLoaders 
{
    /// <sumario>
    /// Este DataLoader é responsável por resolver o problema N+1 ao buscar Autores por ID.
    /// </sumario>
    public class AutorBatchDataLoader : BatchDataLoader<string, Autor>
    {
        private readonly IAutorRepository _autorRepository;
        public AutorBatchDataLoader(
            IAutorRepository autorRepository,
            IBatchScheduler scheduler)
            : base(scheduler, new DataLoaderOptions())
        {
            // O repositório é injetado no construtor para uso na lógica de loteamento.
            _autorRepository = autorRepository;
        }

        /// <sumario>
        /// Método principal que é executado apenas uma vez, após todas as chaves (AutorIds)
        /// que foram coletadas pelo executor do GraphQL.
        /// </sumario>
        /// <returns>Um dicionário mapeando cada ID de volta ao seu respectivo objeto Autor.</returns>
        protected override async Task<IReadOnlyDictionary<string, Autor>> LoadBatchAsync(
            IReadOnlyList<string> Ids,
            CancellationToken cancellationToken)
        {
            var autores = await _autorRepository.GetByIdsAsync(Ids);

            // Mapeamento: Converte a lista de volta para um dicionário, onde a chave é o Id do Autor.
            return autores.ToDictionary(a => a.Id);
        }
    }
}