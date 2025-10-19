using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using HotChocolate.Types;

namespace Artigo.API.GraphQL.Types
{
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

            // CORRIGIDO: ArtigoVersion -> VersaoArtigo
            descriptor.Field(f => f.Version).Type<NonNullType<EnumType<VersaoArtigo>>>().Description("A versão do artigo (e.g., Original, PrimeiraEdicao, Final)."); // FIX: ArtigoVersion -> VersaoArtigo
            descriptor.Field(f => f.DataRegistro).Description("Data e hora em que esta versão foi registrada.");

            // NOVO CAMPO: Midias associadas à versão do histórico
            descriptor.Field(f => f.Midias)
                .Type<NonNullType<ListType<NonNullType<MidiaEntryType>>>>() // Reusa o tipo MidiaEntryType definido em ArtigoType.cs
                .Description("Lista das mídias associadas a esta versão específica do artigo.");
        }
    }
}