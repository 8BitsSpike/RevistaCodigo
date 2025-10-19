﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artigo.Intf.Entities;
using Artigo.Intf.Interfaces;

namespace Artigo.Api.GraphQL.DataLoaders
{
    /// <sumario>
    /// DataLoader responsável por resolver o problema N+1 ao buscar Interacoes (Comentarios)
    /// para multiplos artigos em uma única chamada.
    /// </sumario>
    public class ArticleInteractionsDataLoader : GroupedDataLoader<string, Interaction>
    {
        private readonly IInteractionRepository _interactionRepository;

        /// <sumario>
        /// Construtor que recebe o IInteractionRepository via injeção de dependência.
        /// </sumario>
        public ArticleInteractionsDataLoader(
            IBatchScheduler batchScheduler,
            IInteractionRepository interactionRepository)
            : base(batchScheduler, new DataLoaderOptions())
        {
            _interactionRepository = interactionRepository;
        }

        /// <sumario>
        /// Método principal que é executado apenas uma vez.
        /// Recebe uma lista de Artigo.Ids e deve retornar um ILookup mapeando
        /// cada Artigo.Id para sua lista de Interacoes (Comentarios).
        /// </sumario>
        /// <param name="keys">Uma lista de Artigo.Ids solicitados pelo Schema.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Um ILookup (chave: ArtigoId, valor: IEnumerable<Interaction>).</returns>
        protected override async Task<ILookup<string, Interaction>> LoadGroupedBatchAsync(
            IReadOnlyList<string> keys,
            CancellationToken cancellationToken)
        {
            // 1. Otimização: Chama o repositório UMA ÚNICA VEZ para todos os IDs.
            // O repositório deve buscar todos os comentários onde ArtigoId está na lista 'keys'.
            var interacoes = await _interactionRepository.GetByArtigoIdsAsync(keys);

            // 2. Mapeamento: Converte a lista de volta para um ILookup.
            // O ILookup é essencial para o GroupedDataLoader, pois mapeia uma única chave (ArtigoId)
            // para MÚLTIPLOS valores (a lista de Interacoes/Comentarios).
            return interacoes.ToLookup(i => i.ArtigoId);
        }
    }
}
