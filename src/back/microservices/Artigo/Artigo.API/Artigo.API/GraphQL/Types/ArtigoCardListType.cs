using Artigo.Server.DTOs;
using HotChocolate.Types;

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

            // O campo 'Tipo' foi removido do DTO, conforme a nova especificação.

            descriptor.Field(f => f.MidiaDestaque)
                .Type<MidiaEntryType>() // Reutiliza o MidiaEntryType (definido em ArtigoType.cs)
                .Description("A imagem de destaque (primeira mídia) do artigo.");
        }
    }
}