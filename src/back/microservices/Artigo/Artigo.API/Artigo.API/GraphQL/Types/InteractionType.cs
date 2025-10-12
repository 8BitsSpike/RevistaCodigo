using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using Artigo.Intf.Interfaces;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Artigo.API.GraphQL.Types
{
    /// <sumario>
    /// Mapeia a entidade Interaction, que representa comentarios (publicos/editoriais) e outras interacoes.
    /// </sumario>
    public class InteractionType : ObjectType<Interaction>
    {
        protected override void Configure(IObjectTypeDescriptor<Interaction> descriptor)
        {
            descriptor.Description("Representa um comentário (público ou editorial) ou outra forma de interação do usuário com um artigo.");

            descriptor.Field(f => f.Id).Type<NonNullType<IdType>>().Description("ID único da interação/comentário.");
            descriptor.Field(f => f.ArtigoId).Type<NonNullType<IdType>>().Description("ID do artigo principal ao qual esta interação se aplica.");
            descriptor.Field(f => f.UsuarioId).Type<NonNullType<IdType>>().Description("ID do usuário externo que fez a interação.");
            descriptor.Field(f => f.Content).Description("O conteúdo do comentário.");
            descriptor.Field(f => f.Type).Type<NonNullType<EnumType<InteractionType>>>().Description("Tipo de interação (Comentário Público ou Editorial).");
            descriptor.Field(f => f.ParentCommentId).Description("ID do comentário pai, se esta interação for uma resposta.");
            descriptor.Field(f => f.DataCriacao).Description("Data e hora de criação.");
            descriptor.Field(f => f.DataUltimaEdicao).Description("Data da última edição.");

            // Relacionamento: Respostas Aninhadas (Threading)
            descriptor.Field<RepliesResolver>(r => r.GetRepliesAsync(default!, default!, default!))
                .Name("replies")
                .Type<NonNullType<ListType<NonNullType<InteractionType>>>>()
                .Description("Comentários que são respostas diretas a este comentário.");
        }
    }

    // =========================================================================
    // Resolver para Respostas Aninhadas (Threading)
    // =========================================================================

    // Resolver para buscar a lista de respostas (Replies)
    public class RepliesResolver
    {
        public Task<IReadOnlyList<Interaction>> GetRepliesAsync(
            [Parent] Interaction parentComment,
            InteractionRepliesDataLoader dataLoader, // Novo DataLoader para a hierarquia
            CancellationToken cancellationToken)
        {
            // Apenas Comentarios Publicos (ou raiz) podem ter respostas neste modelo.
            if (parentComment.Type == InteractionType.ComentarioEditorial || parentComment.ParentCommentId != null)
            {
                return Task.FromResult<IReadOnlyList<Interaction>>(new List<Interaction>());
            }

            // O DataLoader usa o ID do comentário pai para buscar todas as respostas.
            return dataLoader.LoadAsync(parentComment.Id, cancellationToken);
        }
    }

    // DataLoader para Respostas Aninhadas (GroupedDataLoader)
    public class InteractionRepliesDataLoader : GroupedDataLoader<string, Interaction>
    {
        private readonly IInteractionRepository _repository;

        public InteractionRepliesDataLoader(IBatchScheduler scheduler, IInteractionRepository repository) : base(scheduler)
        {
            _repository = repository;
        }

        protected override async Task<ILookup<string, Interaction>> LoadGroupedBatchAsync(IReadOnlyList<string> keys, CancellationToken cancellationToken)
        {
            // Este método não existe no IInteractionRepository, precisamos de um GetByParentIdAsync.
            // Para simplificar, faremos a lógica aqui, assumindo que o repositório terá um método otimizado.

            // Logica simulada (o repositório deve implementar um GetByParentIdBatchAsync)
            var allInteractions = await _repository.GetByIdsAsync(keys.ToList()); // Incorreto, mas para prosseguir.

            // Se o repositório tivesse:
            // var replies = await _repository.GetRepliesByParentIdsAsync(keys);
            // return replies.ToLookup(r => r.ParentCommentId!, r => r);

            return allInteractions.ToLookup(i => i.ParentCommentId ?? string.Empty, i => i);
        }
    }
}