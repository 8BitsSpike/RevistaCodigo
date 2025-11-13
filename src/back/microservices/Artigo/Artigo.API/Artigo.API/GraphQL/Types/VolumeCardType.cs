using Artigo.Server.DTOs;
using HotChocolate.Types;

namespace Artigo.API.GraphQL.Types
{
    /// <sumario>
    /// Mapeia o VolumeCardDTO para um tipo de objeto GraphQL.
    /// Representa o 'Volume Card' format, usado pelo ArtigoView.
    /// </sumario>
    public class VolumeCardType : ObjectType<VolumeCardDTO>
    {
        protected override void Configure(IObjectTypeDescriptor<VolumeCardDTO> descriptor)
        {
            descriptor.Description("Representa um volume (edição) em formato de 'card' resumido.");

            descriptor.Field(f => f.Id).Type<NonNullType<IdType>>();
            descriptor.Field(f => f.VolumeTitulo).Type<NonNullType<StringType>>();
            descriptor.Field(f => f.VolumeResumo).Type<NonNullType<StringType>>();

            descriptor.Field(f => f.ImagemCapa)
                .Type<MidiaEntryType>() // Reutiliza o MidiaEntryType (definido em ArtigoType.cs)
                .Description("A imagem de destaque (capa) do volume.");
        }
    }
}