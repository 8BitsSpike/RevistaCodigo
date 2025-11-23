using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using HotChocolate.Types;

namespace Artigo.API.GraphQL.Types
{
    /// <sumario>
    /// Mapeia a entidade StaffComentario para um tipo de objeto GraphQL.
    /// </sumario>
    public class StaffComentarioType : ObjectType<StaffComentario>
    {
        protected override void Configure(IObjectTypeDescriptor<StaffComentario> descriptor)
        {
            descriptor.Description("Representa um comentário editorial interno em uma versão do histórico.");

            descriptor.Field(f => f.Id).Type<NonNullType<IdType>>().Description("ID único do comentário.");
            descriptor.Field(f => f.UsuarioId).Type<NonNullType<IdType>>().Description("ID do usuário (Staff) que fez o comentário.");
            descriptor.Field(f => f.Data).Type<NonNullType<DateTimeType>>().Description("Data e hora do comentário.");
            descriptor.Field(f => f.Parent).Type<IdType>().Description("ID do comentário 'pai' (se for uma resposta).");
            descriptor.Field(f => f.Comment).Type<NonNullType<StringType>>().Description("O conteúdo do comentário.");

            // TODO: Adicionar um resolver para buscar o 'Autor' (Staff) que fez o comentário,
            // usando o 'UsuarioId' e o 'AutorBatchDataLoader'.
        }
    }


    /// <sumario>
    /// Mapeia a entidade ArtigoHistory, que representa uma versão completa do conteúdo de um artigo.
    /// </sumario>
    public class ArtigoHistoryType : ObjectType<ArtigoHistory>
    {
        protected override void Configure(IObjectTypeDescriptor<ArtigoHistory> descriptor)
        {
            descriptor.Description("Representa uma versão histórica (snapshot) do conteúdo do artigo.");

            descriptor.Field(f => f.Id).Type<NonNullType<IdType>>().Description("ID local do registro de histórico.");
            descriptor.Field(f => f.ArtigoId).Type<NonNullType<IdType>>().Description("ID do artigo principal ao qual esta versão pertence.");

            // O campo principal, que contém o corpo completo do artigo.
            descriptor.Field(f => f.Content).Type<NonNullType<StringType>>().Description("O corpo completo e formatado do artigo nesta versão.");

            descriptor.Field(f => f.Version).Type<NonNullType<EnumType<VersaoArtigo>>>().Description("A versão do artigo (e.g., Original, PrimeiraEdicao, Final).");
            descriptor.Field(f => f.DataRegistro).Description("Data e hora em que esta versão foi registrada.");

            // Midias associadas à versão do histórico
            descriptor.Field(f => f.Midias)
                .Type<NonNullType<ListType<NonNullType<MidiaEntryType>>>>() // Reusa o tipo MidiaEntryType definido em ArtigoType.cs
                .Description("Lista das mídias associadas a esta versão específica do artigo.");

            descriptor.Field(f => f.StaffComentarios)
                .Type<NonNullType<ListType<NonNullType<StaffComentarioType>>>>()
                .Description("Lista de comentários internos da equipe editorial sobre esta versão.");
        }
    }
}