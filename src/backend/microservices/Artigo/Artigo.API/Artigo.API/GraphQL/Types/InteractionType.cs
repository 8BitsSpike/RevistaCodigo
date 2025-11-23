using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using Artigo.Intf.Interfaces;
using Artigo.API.GraphQL.Resolvers;
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

            // Nome desnormalizado do usuário
            descriptor.Field(f => f.UsuarioNome)
                .Type<NonNullType<StringType>>()
                .Description("Nome de exibição do autor da interação (denormalizado).");

            descriptor.Field(f => f.Content).Description("O conteúdo do comentário.");
            descriptor.Field(f => f.Type).Type<NonNullType<EnumType<TipoInteracao>>>().Description("Tipo de interação (Comentário Público ou Editorial).");
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
}