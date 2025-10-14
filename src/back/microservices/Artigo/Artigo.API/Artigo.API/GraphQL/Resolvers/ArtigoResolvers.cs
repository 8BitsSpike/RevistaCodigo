using Artigo.Intf.Entities;
using Artigo.Server.DTOs;
using Artigo.API.GraphQL.DataLoaders;
using HotChocolate.Resolvers;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace Artigo.API.GraphQL.Resolvers
{
    // --- EditorialResolver remains the same ---
    public class EditorialResolver
    {
        public Task<Editorial?> GetEditorialAsync(
            [Parent] ArtigoDTO artigo,
            EditorialDataLoader dataLoader,
            IResolverContext context)
        {
            if (string.IsNullOrEmpty(artigo.EditorialId))
            {
                return Task.FromResult<Editorial?>(null);
            }
            // FIX: Asserts the non-null result of the DataLoader load operation.
            return dataLoader.LoadAsync(artigo.EditorialId, context.RequestAborted)!;
        }
    }


    /// <sumario>
    /// Resolver para Autores (N:M).
    /// </sumario>
    public class AutorResolver
    {
        public async Task<IReadOnlyList<Autor>> GetAutoresAsync(
            [Parent] ArtigoDTO artigo,
            AutorDataLoader dataLoader,
            IResolverContext context,
            CancellationToken cancellationToken)
        {
            // 1. Load the ILookup (which is the result type of the GroupedDataLoader)
            IReadOnlyList<string> keys = artigo.AutorIds;
            var lookup = await dataLoader.LoadAsync(keys, cancellationToken);

            // 2. FIX: Flatten the ILookup<string, Autor> into a single IReadOnlyList<Autor>.
            // This explicitly creates the non-array, non-nullable final list type required by the schema.
            return lookup.SelectMany(g => g!).ToList().AsReadOnly();
        }
    }


    /// <sumario>
    /// Resolver para o Conteúdo Atual (ArtigoHistory).
    /// </sumario>
    public class ArtigoHistoryResolver
    {
        public Task<string> GetCurrentContentAsync(
            [Parent] ArtigoDTO artigo,
            CurrentHistoryContentDataLoader dataLoader,
            IResolverContext context)
        {
            if (string.IsNullOrEmpty(artigo.EditorialId))
            {
                return Task.FromResult(string.Empty);
            }
            // FIX: Asserts the non-null result of the DataLoader load operation.
            return dataLoader.LoadAsync(artigo.EditorialId, context.RequestAborted)!;
        }
    }

    // --- VolumeResolver remains the same ---
    public class VolumeResolver
    {
        public Task<Volume?> GetVolumeAsync(
            [Parent] ArtigoDTO artigo,
            VolumeDataLoader dataLoader,
            IResolverContext context)
        {
            if (string.IsNullOrEmpty(artigo.VolumeId))
            {
                return Task.FromResult<Volume?>(null);
            }
            return dataLoader.LoadAsync(artigo.VolumeId!, context.RequestAborted)!;
        }
    }

    /// <sumario>
    /// Resolver para buscar a lista de Artigos por Volume (Usado em VolumeType).
    /// </sumario>
    public class ArticleInVolumeResolver
    {
        public async Task<IReadOnlyList<ArtigoDTO>> GetArticlesAsync(
            [Parent] Volume volume,
            ArtigoGroupedDataLoader dataLoader,
            CancellationToken cancellationToken)
        {
            // 1. Load the ILookup (which is the result type of the GroupedDataLoader)
            IReadOnlyList<string> keys = volume.ArtigoIds;
            var lookup = await dataLoader.LoadAsync(keys, cancellationToken);

            // 2. FIX: Flatten the ILookup<string, ArtigoDTO> into a single IReadOnlyList<ArtigoDTO>.
            // This explicitly creates the non-array, non-nullable final list type required by the schema.
            return lookup.SelectMany(g => g!).ToList().AsReadOnly();
        }
    }
}