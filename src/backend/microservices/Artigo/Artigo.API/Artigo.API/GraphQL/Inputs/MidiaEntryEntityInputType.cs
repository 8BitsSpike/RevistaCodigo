using Artigo.Intf.Entities;
using HotChocolate.Types;

namespace Artigo.API.GraphQL.Inputs
{
    /// <sumario>
    /// Define o tipo de input no GraphQL para a entidade MidiaEntry.
    /// Usado pelo UpdateVolumeMetadataInputType.
    /// </sumario>
    public class MidiaEntryEntityInputType : InputObjectType<MidiaEntry>
    {
        protected override void Configure(IInputObjectTypeDescriptor<MidiaEntry> descriptor)
        {
            descriptor.Description("Dados de uma mídia (imagem, vídeo) associada.");

            descriptor.Field(f => f.MidiaID).Type<NonNullType<StringType>>()
                .Description("ID de referência da mídia.");

            descriptor.Field(f => f.Url).Type<NonNullType<StringType>>()
                .Description("URL de acesso à mídia.");

            descriptor.Field(f => f.Alt).Type<NonNullType<StringType>>()
                .Description("Texto alternativo para SEO e acessibilidade.");
        }
    }
}