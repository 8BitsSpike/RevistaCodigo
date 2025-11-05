using Artigo.Server.DTOs;
using HotChocolate.Types;

namespace Artigo.API.GraphQL.Types
{
    /// <sumario>
    /// Mapeia o VolumeListDTO para um tipo de objeto GraphQL.
    /// Representa o 'Volume List Format'.
    /// </sumario>
    public class VolumeListType : ObjectType<VolumeListDTO>
    {
        protected override void Configure(IObjectTypeDescriptor<VolumeListDTO> descriptor)
        {
            descriptor.Description("Representa um volume (edição) em formato resumido para listas.");

            descriptor.Field(f => f.Id).Type<NonNullType<IdType>>();
            descriptor.Field(f => f.Edicao).Type<NonNullType<IntType>>();
            descriptor.Field(f => f.VolumeTitulo).Type<NonNullType<StringType>>();
            descriptor.Field(f => f.VolumeResumo).Type<NonNullType<StringType>>();
            descriptor.Field(f => f.M).Type<NonNullType<EnumType<Artigo.Intf.Enums.MesVolume>>>();
            descriptor.Field(f => f.N).Type<NonNullType<IntType>>();
            descriptor.Field(f => f.Year).Type<NonNullType<IntType>>();
        }
    }
}