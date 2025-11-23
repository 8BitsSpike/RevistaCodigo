using Artigo.API.GraphQL.DataLoaders;
using Artigo.Intf.Entities;
using Artigo.Server.DTOs;
using HotChocolate.Resolvers;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artigo.Intf.Enums; // Adicionado para TipoInteracao
using HotChocolate; // Adicionado para [Parent] e [Service]

namespace Artigo.API.GraphQL.Resolvers
{
    // --- EditorialResolver ---
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
            // Assegura o resultado não nulo da operação de carregamento do DataLoader.
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
            AutorBatchDataLoader dataLoader,
            IResolverContext context,
            CancellationToken cancellationToken)
        {
            // 1. Carrega as chaves (IDs de Autor).
            IReadOnlyList<string> keys = artigo.IdsAutor;

            // 2. Forço a tipagem para o resultado esperado (IReadOnlyDictionary), devido à complexidade da tipagem
            // assíncrona do HotChocolate, simplificando a projeção final.

            var dictionaryResult = await dataLoader.LoadAsync(keys, cancellationToken);

            // Assegura que o resultado é tratado como um dicionário.
            var dictionary = dictionaryResult as IReadOnlyDictionary<string, Autor>;

            if (dictionary is null)
            {
                // Tratamento de segurança, embora o DataLoader deva retornar o tipo correto.
                return Array.Empty<Autor>().AsReadOnly();
            }

            // 3. Converte os valores do dicionário para uma lista IReadOnlyList<Autor>, 
            // filtrando por chaves solicitadas e valores não nulos.
            var autores = keys
                .Where(dictionary.ContainsKey) // Agora, ContainsKey é válido no dicionário.
                .Select(id => dictionary[id])
                .Where(autor => autor is not null)
                .ToList()
                .AsReadOnly();

            return autores;
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
            // Assegura o resultado não nulo da operação de carregamento do DataLoader.
            return dataLoader.LoadAsync(artigo.EditorialId, context.RequestAborted)!;
        }
    }

    // --- VolumeResolver  ---
    public class VolumeResolver
    {
        public Task<Volume?> GetVolumeAsync(
            [Parent] ArtigoDTO artigo,
            VolumeDataLoader dataLoader,
            IResolverContext context)
        {
            if (string.IsNullOrEmpty(artigo.IdVolume))
            {
                return Task.FromResult<Volume?>(null);
            }
            return dataLoader.LoadAsync(artigo.IdVolume!, context.RequestAborted)!;
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
            // 1. Carrega o ILookup (que é o tipo de resultado do GroupedDataLoader)
            IReadOnlyList<string> keys = volume.ArtigoIds;
            var lookup = await dataLoader.LoadAsync(keys, cancellationToken);

            // 2. Achatamento (Flatten) do ILookup<string, ArtigoDTO> para uma lista única.
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

            // Achatamento (Flatten) do ILookup<string, ArtigoHistory> para uma lista única.
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
            ArticleInteractionsDataLoader dataLoader,
            CancellationToken cancellationToken)
        {
            // ATENÇÃO: É necessário que a entidade Editorial tenha o ArtigoId para carregar os comentários.
            // Esta implementação assume que Editorial.ArtigoId existe.
            if (string.IsNullOrEmpty(editorial.ArtigoId))
            {
                return new List<Interaction>().AsReadOnly();
            }

            // O ArticleInteractionsDataLoader usa o ArtigoId como chave.
            IReadOnlyList<string> keys = new List<string> { editorial.ArtigoId };
            var lookup = await dataLoader.LoadAsync(keys, cancellationToken);

            // Achatamento (Flatten) do ILookup<string, Interaction> para uma lista única.
            return lookup.SelectMany(g => (IEnumerable<Interaction>)g!).ToList().AsReadOnly();
        }
    }

    /// <sumario>
    /// Resolver para buscar respostas/réplicas para uma interação (comentário) "pai" (Usado em InteractionType).
    /// </sumario>
    public class RepliesResolver
    {
        public async Task<IReadOnlyList<Interaction>> GetRepliesAsync(
            [Parent] Interaction parentComment,
            InteractionRepliesDataLoader dataLoader,
            CancellationToken cancellationToken)
        {
            // O DataLoader agrupa respostas por ParentInteractionId, que é o 'Id' do comentário "pai".
            IReadOnlyList<string> keys = new List<string> { parentComment.Id };
            var lookup = await dataLoader.LoadAsync(keys, cancellationToken);

            // Achatamento (Flatten) do ILookup<string, Interaction> para uma lista única.
            return lookup.SelectMany(g => g!).ToList().AsReadOnly();
        }
    }

    /// <sumario>
    /// Lógica centralizada para resolver a conexão de interações (pública/editorial)
    /// para ArtigoViewType e ArtigoEditorialViewType.
    /// </sumario>
    public class ArtigoInteractionsResolver
    {
        public async Task<InteractionConnectionDTO> GetInteractionsAsync(
            [Parent] ArtigoViewDTO dto, // Usa ArtigoViewDTO como base (é compatível)
            int page,
            int pageSize,
            ArticleInteractionsDataLoader dataLoader,
            CancellationToken cancellationToken)
        {
            var interacoes = await dataLoader.LoadAsync(dto.Id, cancellationToken);

            if (interacoes == null || !interacoes.Any())
            {
                return new InteractionConnectionDTO
                {
                    ComentariosEditoriais = new List<Interaction>(),
                    ComentariosPublicos = new List<Interaction>(),
                    TotalComentariosPublicos = 0
                };
            }

            var editoriais = interacoes
                .Where(i => i.Type == TipoInteracao.ComentarioEditorial)
                .ToList();

            var publicos = interacoes
                .Where(i => i.Type == TipoInteracao.ComentarioPublico);

            var publicosPaginados = publicos
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToList();

            return new InteractionConnectionDTO
            {
                ComentariosEditoriais = editoriais,
                ComentariosPublicos = publicosPaginados,
                TotalComentariosPublicos = publicos.Count()
            };
        }
        // Adicione este método para lidar com o DTO Editorial
        public async Task<InteractionConnectionDTO> GetEditorialInteractionsAsync(
        [Parent] ArtigoEditorialViewDTO dto, // <--- O Pai correto
        [Service] ArticleInteractionsDataLoader dataLoader,
        CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(dto.Id)) return new InteractionConnectionDTO();

            var interacoes = await dataLoader.LoadAsync(dto.Id, cancellationToken);

            // Lógica de filtro simples
            if (interacoes == null) return new InteractionConnectionDTO();

            var editoriais = interacoes.Where(i => i.Type == TipoInteracao.ComentarioEditorial).ToList();

            return new InteractionConnectionDTO
            {
                ComentariosEditoriais = editoriais,
                TotalComentariosPublicos = 0
            };
        }
    }

}