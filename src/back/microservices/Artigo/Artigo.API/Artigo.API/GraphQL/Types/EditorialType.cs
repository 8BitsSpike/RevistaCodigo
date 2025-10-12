using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using Artigo.Intf.Interfaces;
using HotChocolate.Types;
using System.Collections.Generic;

namespace Artigo.API.GraphQL.Types
{
    /// <sumario>
    /// Tipo embutido: Representa a equipe de revisao e edicao designada para o artigo.
    /// </sumario>
    public class EditorialTeamType : ObjectType<EditorialTeam>
    {
        protected override void Configure(IObjectTypeDescriptor<EditorialTeam> descriptor)
        {
            descriptor.Description("A equipe editorial designada, incluindo revisores, corretores e o editor chefe.");

            // Os IDs de usuário/autor serão resolvidos para o tipo AutorType ou StaffType em resolvers futuros.
            descriptor.Field(f => f.InitialAuthorId).Description("IDs dos autores principais envolvidos.");
            descriptor.Field(f => f.EditorId).Description("ID do Editor Chefe responsável.");
            descriptor.Field(f => f.ReviewerIds).Description("IDs dos revisores designados.");
            descriptor.Field(f => f.CorrectorIds).Description("IDs dos corretores designados.");
        }
    }

    /// <sumario>
    /// Mapeia a entidade Editorial para um tipo de objeto GraphQL, focando no ciclo de vida.
    /// </sumario>
    public class EditorialType : ObjectType<Editorial>
    {
        protected override void Configure(IObjectTypeDescriptor<Editorial> descriptor)
        {
            descriptor.Description("O registro que rastreia a posição e o histórico de revisões de um artigo.");

            descriptor.Field(f => f.Id).Type<NonNullType<IdType>>().Description("ID local do registro editorial.");
            descriptor.Field(f => f.ArtigoId).Type<NonNullType<IdType>>().Description("ID do artigo associado.");
            descriptor.Field(f => f.Position).Type<NonNullType<EnumType<EditorialPosition>>>().Description("Posição atual no fluxo de trabalho (e.g., AwaitingReview).");
            descriptor.Field(f => f.CurrentHistoryId).Description("ID da versão atual do conteúdo (ArtigoHistory).");
            descriptor.Field(f => f.LastUpdated).Description("Data da última atualização de status.");

            // 1. Objeto Embutido: Equipe
            descriptor.Field(f => f.Team)
                .Type<NonNullType<EditorialTeamType>>()
                .Description("A equipe editorial designada para o artigo.");

            // 2. Histórico de Conteúdo (Lista de Versões)
            // Este campo usará um DataLoader para buscar todos os ArtigoHistory associados.
            descriptor.Field<ArtigoHistoryListResolver>(r => r.GetHistoryAsync(default!, default!, default!))
                .Name("history")
                .Type<NonNullType<ListType<NonNullType<ArtigoHistoryType>>>>() // Presume a existência futura do ArtigoHistoryType
                .Description("Lista de todas as versões históricas do conteúdo deste artigo.");

            // 3. Comentários Editoriais
            // Este campo usará um DataLoader para buscar todos os Comments/Interactions com InteractionType.ComentarioEditorial.
            descriptor.Field<InteractionListResolver>(r => r.GetEditorialCommentsAsync(default!, default!, default!))
                .Name("comments")
                .Type<NonNullType<ListType<NonNullType<InteractionType>>>>() // Presume a existência futura do InteractionType
                .Description("Comentários internos feitos pela equipe editorial sobre o artigo.");
        }
    }

    // =========================================================================
    // Resolvers (Simplificados para esta estrutura)
    // =========================================================================

    // Resolver para buscar a lista de ArtigoHistory
    public class ArtigoHistoryListResolver
    {
        public Task<IReadOnlyList<ArtigoHistory>> GetHistoryAsync(
            [Parent] Editorial editorial,
            ArtigoHistoryGroupedDataLoader dataLoader,
            CancellationToken cancellationToken)
        {
            // O DataLoader usará a lista de HistoryIds do Editorial para buscar todas as versões.
            return dataLoader.LoadAsync(editorial.HistoryIds, cancellationToken);
        }
    }

    // DataLoader para ArtigoHistory (GroupedDataLoader para listas)
    // Este DataLoader precisa ser criado na pasta Artigo.API/GraphQL/DataLoaders/
    public class ArtigoHistoryGroupedDataLoader : GroupedDataLoader<string, ArtigoHistory>
    {
        private readonly IArtigoHistoryRepository _repository;

        public ArtigoHistoryGroupedDataLoader(IBatchScheduler scheduler, IArtigoHistoryRepository repository) : base(scheduler)
        {
            _repository = repository;
        }

        protected override async Task<ILookup<string, ArtigoHistory>> LoadGroupedBatchAsync(IReadOnlyList<string> keys, CancellationToken cancellationToken)
        {
            // keys é a lista consolidada de todos os HistoryIds
            var historyEntries = await _repository.GetByIdsAsync(keys.ToList());

            // Retorna como ILookup, onde a chave é o ArtigoHistory.Id
            return historyEntries.ToLookup(h => h.Id, h => h);
        }
    }

    // Resolver para buscar a lista de Comentários Editoriais
    public class InteractionListResolver
    {
        public Task<IReadOnlyList<Interaction>> GetEditorialCommentsAsync(
            [Parent] Editorial editorial,
            InteractionDataLoader dataLoader,
            CancellationToken cancellationToken)
        {
            // O DataLoader usará a lista de CommentIds do Editorial para buscar os comentários editoriais.
            return dataLoader.LoadAsync(editorial.CommentIds, cancellationToken);
        }
    }

    // DataLoader para Interaction (BatchDataLoader para IDs)
    // Este DataLoader precisa ser criado na pasta Artigo.API/GraphQL/DataLoaders/
    public class InteractionDataLoader : BatchDataLoader<string, Interaction>
    {
        private readonly IInteractionRepository _repository;

        public InteractionDataLoader(IBatchScheduler scheduler, IInteractionRepository repository) : base(scheduler)
        {
            _repository = repository;
        }

        protected override async Task<IReadOnlyDictionary<string, Interaction>> LoadBatchAsync(IReadOnlyList<string> keys, CancellationToken cancellationToken)
        {
            var interactions = await _repository.GetByIdsAsync(keys.ToList());
            return interactions.ToDictionary(i => i.Id);
        }
    }
}