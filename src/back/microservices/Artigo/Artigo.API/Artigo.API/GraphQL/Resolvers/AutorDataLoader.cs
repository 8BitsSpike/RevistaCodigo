using Artigo.Intf.Entities;
using Artigo.Intf.Interfaces;


namespace Artigo.Api.GraphQL.Resolvers
{
    /// <sumario>
    /// DataLoader responsável por resolver o problema N+1 ao buscar Autores por ID.
    /// </sumario>
    public class AutorDataLoader : BatchDataLoader<string, Autor>
    {
        private readonly IAutorRepository _autorRepository;

        /// <sumario>
        /// Construtor que recebe as dependências necessárias.
        /// A injeção de IServiceProvider resolve o problema CS7036/ArgumentNullException,
        /// pois é o padrão estável do HotChocolate para inicializar o BatchDataLoader.
        /// </sumario>
        public AutorDataLoader(
            IAutorRepository autorRepository,
            IServiceProvider serviceProvider)
            // Chamada base que satisfaz a classe BatchDataLoader.
            // O HotChocolate usa o IServiceProvider para resolver o IBatchScheduler internamente.
            : base(serviceProvider)
        {
            // O repositório é injetado no construtor para uso na lógica de loteamento.
            _autorRepository = autorRepository;
        }

        /// <sumario>
        /// Método principal que é executado apenas uma vez, após todas as chaves (AutorIds)
        /// terem sido coletadas pelo executor do GraphQL.
        /// </sumario>
        /// <param name="keys">Uma lista de Autor.Ids solicitados por vários Artigos.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Um dicionário mapeando cada ID de volta ao seu respectivo objeto Autor.</returns>
        protected override async Task<IReadOnlyDictionary<string, Autor>> LoadBatchAsync(
            IReadOnlyList<string> keys,
            CancellationToken cancellationToken)
        {
            // 1. Otimização: Chama o repositório UMA ÚNICA VEZ para todos os IDs.
            var autores = await _autorRepository.GetByIdsAsync(keys, cancellationToken);

            // 2. Mapeamento: Converte a lista de volta para um dicionário, onde a chave é o Id do Autor.
            return autores.ToDictionary(a => a.Id);
        }
    }
}
