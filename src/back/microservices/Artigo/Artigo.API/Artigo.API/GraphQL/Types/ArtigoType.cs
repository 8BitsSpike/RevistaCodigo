using Artigo.Api.GraphQL.Types;
using Artigo.API.GraphQL.DataLoaders;
using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using Artigo.Intf.Interfaces;
using Artigo.Server.DTOs;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Artigo.API.GraphQL.Types
{
    /// <sumario>
    /// Mapeia o ArtigoDTO para um tipo de objeto GraphQL, definindo as bordas (edges) de relacionamento.
    /// </sumario>
    public class ArtigoType : ObjectType<ArtigoDTO>
    {
        protected override void Configure(IObjectTypeDescriptor<ArtigoDTO> descriptor)
        {
            descriptor.Description("Representa um artigo da revista, incluindo metadados e status editorial.");

            // Campos primários (Mapeamento direto do DTO)
            descriptor.Field(f => f.Id).Type<NonNullType<IdType>>().Description("O ID único do artigo.");
            descriptor.Field(f => f.Titulo).Description("Título principal do artigo.");
            descriptor.Field(f => f.Resumo).Description("Resumo/Abstract do conteúdo.");
            descriptor.Field(f => f.Status).Type<NonNullType<EnumType<ArtigoStatus>>>().Description("Status do ciclo de vida editorial.");
            descriptor.Field(f => f.Tipo).Type<NonNullType<EnumType<ArtigoTipo>>>().Description("Classificação do tipo de artigo.");

            // Campos Denormalizados/Métricas
            descriptor.Field(f => f.TotalComentarios).Description("Contagem total de comentários públicos (Denormalizado).");
            descriptor.Field(f => f.TotalInteracoes).Description("Contagem total de interações (Denormalizado).");

            // Relacionamentos Resolvidos (DataLoaders)

            // 1. Editorial (1:1)
            descriptor.Field<EditorialResolver>(r => r.GetEditorialAsync(default!, default!, default!))
                .Name("editorial")
                .Type<NonNullType<EditorialType>>() // Presume a existência futura do EditorialType
                .Description("O registro que gerencia o ciclo de vida editorial e revisões do artigo.");

            // 2. Autores (N:M)
            descriptor.Field<AutorResolver>(r => r.GetAutoresAsync(default!, default!, default!, default!))
                .Name("autores")
                .Type<NonNullType<ListType<NonNullType<AutorType>>>>() // Presume a existência futura do AutorType
                .Description("Os autores e co-autores cadastrados responsáveis pela criação do artigo.");

            // 3. Conteúdo (1:N para ArtigoHistory - Busca a versão atual)
            descriptor.Field<ArtigoHistoryResolver>(r => r.GetCurrentContentAsync(default!, default!, default!))
                .Name("currentContent")
                .Type<StringType>()
                .Description("O conteúdo da versão atual do artigo.");

            // 4. Volume (1:1 Opcional - Busca o Volume apenas se VolumeId existir)
            descriptor.Field<VolumeResolver>(r => r.GetVolumeAsync(default!, default!, default!))
                .Name("volumePublicado")
                .Type<VolumeType>() // Tipo opcional
                .Description("O Volume (edição da revista) no qual o artigo foi publicado, se aplicável.");
        }
    }

    // =========================================================================
    // Resolvers (Para resolver campos de relacionamento)
    // =========================================================================

    // Nota: Estes resolvers serão movidos para a pasta Artigo.API/GraphQL/Resolvers/
    // A estrutura aqui é simplificada para referência.

    // Resolver para o Editorial (1:1)
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
            // Usa o DataLoader para carregar o Editorial pelo ID
            return dataLoader.LoadAsync(artigo.EditorialId, context.RequestAborted);
        }
    }

    // Resolver para o Conteúdo Atual (ArtigoHistory)
    public class ArtigoHistoryResolver
    {
        public Task<string> GetCurrentContentAsync(
            [Parent] ArtigoDTO artigo,
            CurrentHistoryContentDataLoader dataLoader,
            IResolverContext context)
        {
            // O DataLoader buscará o ArtigoHistoryModel correspondente ao CurrentHistoryId
            // na coleção Editorial (que o ArtigoService teria carregado).
            // Aqui, usamos o EditorialId para carregar a informação de história.

            if (string.IsNullOrEmpty(artigo.EditorialId))
            {
                return Task.FromResult(string.Empty);
            }
            // O DataLoader usará o EditorialId para encontrar o Editorial e, a partir dele, 
            // buscar o ArtigoHistory atual (CurrentHistoryId).
            return dataLoader.LoadAsync(artigo.EditorialId, context.RequestAborted);
        }
    }

    // =========================================================================
    // DataLoaders (Ainda nao criados, mas referenciados)
    // =========================================================================

    // Nota: Estes DataLoaders precisarão ser criados na pasta Artigo.API/GraphQL/DataLoaders/

    // DataLoader para o Editorial
    public class EditorialDataLoader : BatchDataLoader<string, Editorial>
    {
        private readonly IEditorialRepository _repository;
        public EditorialDataLoader(IBatchScheduler scheduler, IEditorialRepository repository) : base(scheduler)
        {
            _repository = repository;
        }
        protected override async Task<IReadOnlyDictionary<string, Editorial>> LoadBatchAsync(IReadOnlyList<string> keys, CancellationToken cancellationToken)
        {
            var editorials = await _repository.GetByIdsAsync(keys.ToList());
            return editorials.ToDictionary(e => e.Id);
        }
    }

    // DataLoader para o Conteúdo Atual (mais complexo)
    public class CurrentHistoryContentDataLoader : BatchDataLoader<string, string>
    {
        private readonly IEditorialRepository _editorialRepository;
        private readonly IArtigoHistoryRepository _historyRepository;

        public CurrentHistoryContentDataLoader(IBatchScheduler scheduler, IEditorialRepository editorialRepository, IArtigoHistoryRepository historyRepository) : base(scheduler)
        {
            _editorialRepository = editorialRepository;
            _historyRepository = historyRepository;
        }

        protected override async Task<IReadOnlyDictionary<string, string>> LoadBatchAsync(IReadOnlyList<string> keys, CancellationToken cancellationToken)
        {
            // keys = Lista de EditorialIds
            var editorials = await _editorialRepository.GetByIdsAsync(keys);

            // Extrai todos os CurrentHistoryIds necessários
            var historyIds = editorials.Select(e => e.CurrentHistoryId).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();

            // Busca todos os ArtigoHistory em lote
            var histories = await _historyRepository.GetByIdsAsync(historyIds);
            var historyLookup = histories.ToDictionary(h => h.Id, h => h.Content);

            var result = new Dictionary<string, string>();

            foreach (var editorial in editorials)
            {
                // Mapeia o conteúdo de volta para o EditorialId original (a chave)
                string content = string.Empty;
                if (historyLookup.TryGetValue(editorial.CurrentHistoryId, out var hContent))
                {
                    content = hContent;
                }
                result[editorial.Id] = content;
            }

            return result;
        }
    }

    // Resolver para Volume (Opcional 1:1)
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
            return dataLoader.LoadAsync(artigo.VolumeId, context.RequestAborted);
        }
    }

    // DataLoader para Volume
    public class VolumeDataLoader : BatchDataLoader<string, Volume>
    {
        private readonly IVolumeRepository _repository;
        public VolumeDataLoader(IBatchScheduler scheduler, IVolumeRepository repository) : base(scheduler)
        {
            _repository = repository;
        }
        protected override async Task<IReadOnlyDictionary<string, Volume>> LoadBatchAsync(IReadOnlyList<string> keys, CancellationToken cancellationToken)
        {
            var volumes = await _repository.GetByIdsAsync(keys.ToList());
            return volumes.ToDictionary(v => v.Id);
        }
    }

    // Resolver para Autores (N:M)
    public class AutorResolver
    {
        public Task<IReadOnlyList<Autor>> GetAutoresAsync(
            [Parent] ArtigoDTO artigo,
            AutorDataLoader dataLoader,
            IResolverContext context,
            CancellationToken cancellationToken)
        {
            // O DataLoader lida com a lista de IDs
            return dataLoader.LoadAsync(artigo.AutorIds, cancellationToken);
        }
    }

    // DataLoader para Autores
    public class AutorDataLoader : GroupedDataLoader<string, Autor>
    {
        private readonly IAutorRepository _repository;

        public AutorDataLoader(IBatchScheduler scheduler, IAutorRepository repository) : base(scheduler)
        {
            _repository = repository;
        }

        protected override async Task<ILookup<string, Autor>> LoadGroupedBatchAsync(IReadOnlyList<string> keys, CancellationToken cancellationToken)
        {
            // Keys é a lista consolidada de todos os AutorIds
            var autores = await _repository.GetByIdsAsync(keys, cancellationToken);

            // Retorna como ILookup, onde a chave é o Autor.Id
            return autores.ToLookup(a => a.Id, a => a);
        }
    }
}