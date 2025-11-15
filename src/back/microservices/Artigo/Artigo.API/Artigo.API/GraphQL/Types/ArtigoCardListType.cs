using Artigo.Server.DTOs;
using HotChocolate.Types;
using Artigo.Intf.Enums; 

namespace Artigo.API.GraphQL.Types
{
    /// <sumario>
    /// Mapeia o ArtigoCardListDTO para um tipo de objeto GraphQL.
    /// Representa o 'Artigo Card' format.
    /// </sumario>
    public class ArtigoCardListType : ObjectType<ArtigoCardListDTO>
    {
        protected override void Configure(IObjectTypeDescriptor<ArtigoCardListDTO> descriptor)
        {
            descriptor.Description("Representa um artigo em formato de 'card' resumido para listas.");

            descriptor.Field(f => f.Id).Type<NonNullType<IdType>>();
            descriptor.Field(f => f.Titulo).Type<NonNullType<StringType>>();
            descriptor.Field(f => f.Resumo).Type<NonNullType<StringType>>();
            descriptor.Field(f => f.Status)
                .Type<NonNullType<EnumType<StatusArtigo>>>()
                .Description("O status editorial atual do artigo.");

            descriptor.Field(f => f.Tipo)
                .Type<NonNullType<EnumType<TipoArtigo>>>()
                .Description("O tipo do artigo (e.g., Artigo, Blog).");

            descriptor.Field(f => f.PermitirComentario)
                .Type<NonNullType<BooleanType>>()
                .Description("Indica se comentários públicos estão permitidos.");

            descriptor.Field(f => f.MidiaDestaque)
                .Type<MidiaEntryType>() // Reutiliza o MidiaEntryType (definido em ArtigoType.cs)
                .Description("A imagem de destaque (primeira mídia) do artigo.");
        }
    }
}