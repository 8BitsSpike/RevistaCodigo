using Artigo.Intf.Enums;
using HotChocolate.Types;
using System.Collections.Generic;
using Artigo.Intf.Inputs;

namespace Artigo.API.GraphQL.Inputs
{
    /// <sumario>
    /// Data Transfer Object (DTO) de input para a atualização de metadados de um Artigo.
    /// Define os campos que podem ser modificados após a criação.
    /// </sumario>
    public class UpdateArtigoInput : InputObjectType<UpdateArtigoMetadataInput>
    {
        protected override void Configure(IInputObjectTypeDescriptor<UpdateArtigoMetadataInput> descriptor)
        {
            descriptor.Description("Dados para atualizar o título, resumo e lista de autores de um artigo.");

            // Nenhum campo é estritamente obrigatório (NonNullType) para permitir atualizações parciais.
            descriptor.Field(f => f.Titulo).Type<StringType>();
            descriptor.Field(f => f.Resumo).Type<StringType>();
            descriptor.Field(f => f.Tipo).Type<EnumType<TipoArtigo>>();
            descriptor.Field(f => f.IdsAutor).Type<ListType<IdType>>();
            descriptor.Field(f => f.ReferenciasAutor).Type<ListType<StringType>>();
            descriptor.Field(f => f.Status).Type<EnumType<StatusArtigo>>()
                .Description("Atualiza o status do ciclo de vida do artigo (ex: Publicado, Arquivado).");
            descriptor.Field(f => f.PermitirComentario).Type<BooleanType>()
                .Description("Habilita ou desabilita comentários públicos neste artigo.");
            descriptor.Field(f => f.Posicao).Type<EnumType<PosicaoEditorial>>()
                .Description("Atualiza a posição do artigo no fluxo de trabalho editorial (ex: EmRevisao, ProntoParaPublicar).");
        }
    }
}