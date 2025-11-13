using Artigo.Server.DTOs;
using HotChocolate.Types;

namespace Artigo.API.GraphQL.Inputs
{
    /// <sumario>
    /// Define o tipo de input no GraphQL para uma entrada de Mídia.
    /// Mapeia para o MidiaEntryInputDTO.
    /// </sumario>
    public class MidiaEntryInputType : InputObjectType<MidiaEntryInputDTO>
    {
        protected override void Configure(IInputObjectTypeDescriptor<MidiaEntryInputDTO> descriptor)
        {
            descriptor.Description("Dados de uma mídia (imagem, vídeo) a ser associada ao artigo.");

            descriptor.Field(f => f.MidiaID).Type<NonNullType<StringType>>()
                .Description("ID de referência da mídia (ex: ID do S3 ou outro serviço).");

            descriptor.Field(f => f.Url).Type<NonNullType<StringType>>()
                .Description("URL de acesso à mídia.");

            descriptor.Field(f => f.Alt).Type<NonNullType<StringType>>()
                .Description("Texto alternativo para SEO e acessibilidade.");
        }
    }
}