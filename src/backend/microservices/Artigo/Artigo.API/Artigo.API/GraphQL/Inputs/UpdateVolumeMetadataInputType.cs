using Artigo.Intf.Enums;
using Artigo.Intf.Inputs; // *** ATUALIZADO para Intf.Inputs ***
using HotChocolate.Types;

namespace Artigo.API.GraphQL.Inputs
{
    /// <sumario>
    /// Define o tipo de input no GraphQL para a atualização de metadados de um Volume.
    /// Mapeia para o UpdateVolumeMetadataInput DTO do domínio.
    /// </sumario>
    public class UpdateVolumeMetadataInputType : InputObjectType<UpdateVolumeMetadataInput>
    {
        protected override void Configure(IInputObjectTypeDescriptor<UpdateVolumeMetadataInput> descriptor)
        {
            descriptor.Description("Dados para atualizar os metadados de um Volume (edição). Campos nulos serão ignorados.");

            // Metadados
            descriptor.Field(f => f.Edicao).Type<IntType>();
            descriptor.Field(f => f.VolumeTitulo).Type<StringType>();
            descriptor.Field(f => f.VolumeResumo).Type<StringType>();
            descriptor.Field(f => f.M).Type<EnumType<MesVolume>>();
            descriptor.Field(f => f.N).Type<IntType>();
            descriptor.Field(f => f.Year).Type<IntType>();

            // Novo campo de Status
            descriptor.Field(f => f.Status).Type<EnumType<StatusVolume>>();

            // Lista de Artigos
            descriptor.Field(f => f.ArtigoIds).Type<ListType<NonNullType<IdType>>>();

            // Mídia de Capa
            descriptor.Field(f => f.ImagemCapa)
                .Type<MidiaEntryEntityInputType>() // <-- CORRIGIDO para o tipo de input da entidade
                .Description("Define a nova imagem de capa para o volume.");
        }
    }
}