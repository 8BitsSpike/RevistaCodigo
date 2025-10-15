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
    /// <sumario>
    /// Resolver para buscar o histórico de revisões associadas a um Editorial (Usado em EditorialType).
    /// </sumario>
    public class ArtigoHistoryListResolver
    {
        public async Task<IReadOnlyList<ArtigoHistory>> GetHistoryAsync(
            [Parent] Editorial editorial,
            ArtigoHistoryGroupedDataLoader dataLoader,
            CancellationToken cancellationToken)
        {
            // O DataLoader agrupa ArtigoHistory por EditorialId, que é o campo 'Id' do Editorial.
            IReadOnlyList<string> keys = new List<string> { editorial.Id };
            var lookup = await dataLoader.LoadAsync(keys, cancellationToken);

            // Flatten the ILookup<string, ArtigoHistory> into a single IReadOnlyList<ArtigoHistory>.
            return lookup.SelectMany(g => g!).ToList().AsReadOnly();
        }
    }

    /// <sumario>
    /// Resolver para buscar interações/comentários associados a um Editorial (Usado em EditorialType).
    /// Depende de ArtigoId ser acessível via Editorial, que é usado como chave para o DataLoader.
    /// </sumario>
    public class InteractionListResolver
    {
        public async Task<IReadOnlyList<Interaction>> GetEditorialCommentsAsync(
            [Parent] Editorial editorial,
            InteractionDataLoader dataLoader,
            CancellationToken cancellationToken)
        {
            // ATENÇÃO: É necessário que a entidade Editorial tenha o ArtigoId para carregar os comentários.
            // Esta implementação assume que Editorial.ArtigoId existe.
            if (string.IsNullOrEmpty(editorial.ArtigoId))
            {
                return new List<Interaction>().AsReadOnly();
            }

            // InteractionDataLoader (se for GroupedDataLoader) usa o ArtigoId como chave.
            IReadOnlyList<string> keys = new List<string> { editorial.ArtigoId };
            var lookup = await dataLoader.LoadAsync(keys, cancellationToken);

            // Flatten the ILookup<string, Interaction> into a single IReadOnlyList<Interaction>.
            return lookup.SelectMany(g => (IEnumerable<Interaction>)g!).ToList().AsReadOnly();
        }
    }

    /// <sumario>
    /// Resolver para buscar respostas/réplicas para uma interação (comentário) pai (Usado em InteractionType).
    /// </sumario>
    public class RepliesResolver
    {
        public async Task<IReadOnlyList<Interaction>> GetRepliesAsync(
            [Parent] Interaction parentComment,
            InteractionRepliesDataLoader dataLoader,
            CancellationToken cancellationToken)
        {
            // O DataLoader agrupa respostas por ParentInteractionId, que é o 'Id' do comentário pai.
            IReadOnlyList<string> keys = new List<string> { parentComment.Id };
            var lookup = await dataLoader.LoadAsync(keys, cancellationToken);

            // Flatten the ILookup<string, Interaction> into a single IReadOnlyList<Interaction>.
            return lookup.SelectMany(g => g!).ToList().AsReadOnly();
        }
    }
}