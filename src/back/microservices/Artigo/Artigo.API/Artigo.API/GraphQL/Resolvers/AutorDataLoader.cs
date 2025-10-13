﻿using Artigo.Intf.Entities;
using Artigo.Intf.Interfaces;


namespace Artigo.Api.GraphQL.Resolvers
{
    /// <sumario>
    /// DataLoader responsável por resolver o problema N+1 ao buscar Autores por ID.
    /// </sumario>
    public class AutorDataLoader : BatchDataLoader<string, Autor>
    {
        private readonly IAutorRepository _autorRepository;
        public AutorDataLoader(
            IAutorRepository autorRepository,
            IBatchScheduler scheduler)
            : base(scheduler, new DataLoaderOptions())
        {
            // O repositório é injetado no construtor para uso na lógica de loteamento.
            _autorRepository = autorRepository;
        }

        /// <sumario>
        /// Método principal que é executado apenas uma vez, após todas as chaves (AutorIds)
        /// terem sido coletadas pelo executor do GraphQL.
        /// </sumario>
        /// <returns>Um dicionário mapeando cada ID de volta ao seu respectivo objeto Autor.</returns>
        protected override async Task<IReadOnlyDictionary<string, Autor>> LoadBatchAsync(
            IReadOnlyList<string> Ids,
            CancellationToken cancellationToken)
        {
            // 1. Otimização: Chama o repositório UMA ÚNICA VEZ para todos os IDs.
            var autores = await _autorRepository.GetByIdsAsync(Ids);

            // 2. Mapeamento: Converte a lista de volta para um dicionário, onde a chave é o Id do Autor.
            return autores.ToDictionary(a => a.Id);
        }
    }
}
