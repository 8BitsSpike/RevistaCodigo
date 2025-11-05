using Artigo.Intf.Enums;
using HotChocolate.Types;
using System.Collections.Generic;
using Artigo.Intf.Inputs; // *** ADICIONADO ***

namespace Artigo.API.GraphQL.Inputs
{
    /// <sumario>
    /// Data Transfer Object (DTO) de input para a atualização de metadados de um Artigo.
    /// Define os campos que podem ser modificados após a criação.
    /// *** ATENÇÃO: Esta classe foi movida para Artigo.Intf.Inputs ***
    /// A classe interna foi removida.
    /// </sumario>
    // public class UpdateArtigoMetadataInput
    // { ... }

    /// <sumario>
    /// Define o tipo de input GraphQL para a atualização de metadados de um Artigo.
    /// *** ATUALIZADO: Agora mapeia para a classe do Artigo.Intf.Inputs ***
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
        }
    }
}