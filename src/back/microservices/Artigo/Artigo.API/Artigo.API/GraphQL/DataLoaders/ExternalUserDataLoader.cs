using Artigo.Server.DTOs;
using Artigo.Server.Interfaces;
using GreenDonut;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Artigo.API.GraphQL.DataLoaders
{
    /// <sumario>
    /// DataLoader para buscar perfis de usuário (Nome, Mídia) em lote do UsuarioAPI.
    /// A chave de busca é o UsuarioId externo.
    /// </sumario>
    public class ExternalUserDataLoader : BatchDataLoader<string, ExternalUserDTO>
    {
        private readonly IExternalUserService _externalUserService;

        public ExternalUserDataLoader(
            IBatchScheduler scheduler,
            IExternalUserService externalUserService)
            : base(scheduler, new DataLoaderOptions())
        {
            _externalUserService = externalUserService;
        }

        protected override async Task<IReadOnlyDictionary<string, ExternalUserDTO>> LoadBatchAsync(
            IReadOnlyList<string> keys,
            CancellationToken cancellationToken)
        {
            // O service faz a chamada otimizada para o UsuarioAPI
            var externalUsers = await _externalUserService.GetUsersByIdsAsync(keys.ToList());

            // Mapeia os resultados de volta para um dicionário, usando o UsuarioId como chave.
            return externalUsers.ToDictionary(u => u.UsuarioId);
        }
    }
}