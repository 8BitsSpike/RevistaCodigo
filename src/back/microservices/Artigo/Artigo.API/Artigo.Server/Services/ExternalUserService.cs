using Artigo.Server.DTOs;
using Artigo.Server.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Artigo.Server.Services
{
    /// <sumario>
    /// Implementação STUB do serviço que busca informações de usuários no sistema externo (UsuarioAPI).
    /// Esta versão retorna dados mockados para permitir que o GraphQL/DataLoaders funcionem.
    /// </sumario>
    public class ExternalUserService : IExternalUserService
    {
        // Mock data to simulate API response
        private readonly Dictionary<string, ExternalUserDTO> _mockUsers = new Dictionary<string, ExternalUserDTO>
        {
            { "user_101", new ExternalUserDTO { UsuarioId = "user_101", Name = "Afonso Editor", MediaUrl = "avatar/afonso.jpg" } },
            { "user_102", new ExternalUserDTO { UsuarioId = "user_102", Name = "Beatriz Autora", MediaUrl = "avatar/beatriz.jpg" } },
            { "user_103", new ExternalUserDTO { UsuarioId = "user_103", Name = "Carlos Revisor", MediaUrl = "avatar/carlos.jpg" } },
            // Adicione mais usuários mockados conforme necessário para Staff/Autores
        };

        public Task<ExternalUserDTO?> GetUserByIdAsync(string usuarioId)
        {
            _mockUsers.TryGetValue(usuarioId, out var user);
            return Task.FromResult(user);
        }

        public Task<IReadOnlyList<ExternalUserDTO>> GetUsersByIdsAsync(IReadOnlyList<string> usuarioIds)
        {
            var results = usuarioIds
                .Select(id => GetUserByIdAsync(id).Result) // Note: .Result is used here as a placeholder for synchronization in the mock environment.
                .Where(user => user != null)
                .ToList();

            // The cast to IReadOnlyList<ExternalUserDTO> is necessary for type compliance.
            return Task.FromResult<IReadOnlyList<ExternalUserDTO>>(results!);
        }
    }
}