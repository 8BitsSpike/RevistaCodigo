using Artigo.Server.DTOs;
using Artigo.Intf.Enums;
using HotChocolate.Types;

namespace Artigo.API.GraphQL.Inputs
{
    /// <sumario>
    /// Define o tipo de input no GraphQL para a criação de um novo Volume.
    /// Mapeia para o DTO CreateVolumeRequest.
    /// </sumario>
    public class CreateVolumeInputType : InputObjectType<CreateVolumeRequest>
    {
        protected override void Configure(IInputObjectTypeDescriptor<CreateVolumeRequest> descriptor)
        {
            descriptor.Description("Dados necessários para criar um novo Volume (edição) da revista.");

            descriptor.Field(f => f.Edicao).Type<NonNullType<IntType>>();
            descriptor.Field(f => f.VolumeTitulo).Type<NonNullType<StringType>>();
            descriptor.Field(f => f.VolumeResumo).Type<StringType>(); // Permite resumo nulo/vazio
            descriptor.Field(f => f.M).Type<NonNullType<EnumType<MesVolume>>>();
            descriptor.Field(f => f.N).Type<NonNullType<IntType>>();
            descriptor.Field(f => f.Year).Type<NonNullType<IntType>>();
        }
    }
}