using Artigo.Intf.Enums;
using HotChocolate.Types;
using System.Collections.Generic;

namespace Artigo.API.GraphQL.Inputs
{
    /// <sumario>
    /// Data Transfer Object (DTO) de input para a atualização de metadados de um Artigo.
    /// Define os campos que podem ser modificados após a criação.
    /// </sumario>
    public class UpdateArtigoMetadataInput
    {
        // Metadados principais
        public string? Titulo { get; set; }
        public string? Resumo { get; set; }

        // Tipo de artigo (Enum)
        public ArtigoTipo? Tipo { get; set; }

        // Autores: Permite adicionar ou remover autores/co-autores, mas como uma lista completa.
        public List<string>? AutorIds { get; set; }
        public List<string>? AutorReference { get; set; }
    }

    /// <sumario>
    /// Define o tipo de input GraphQL para a atualização de metadados de um Artigo.
    /// </sumario>
    public class UpdateArtigoInput : InputObjectType<UpdateArtigoMetadataInput>
    {
        protected override void Configure(IInputObjectTypeDescriptor<UpdateArtigoMetadataInput> descriptor)
        {
            descriptor.Description("Dados para atualizar o título, resumo e lista de autores de um artigo.");

            // Nenhum campo é estritamente obrigatório (NonNullType) para permitir atualizações parciais.
        }
    }
}