using Artigo.Server.DTOs;
using HotChocolate.Types;

namespace Artigo.API.GraphQL.Inputs
{
    /// <sumario>
    /// Define o tipo de input no GraphQL para a criação de um novo Artigo.
    /// Mapeia diretamente para o DTO de requisição da camada de aplicação.
    /// </sumario>
    public class CreateArtigoInput : InputObjectType<CreateArtigoRequest>
    {
        protected override void Configure(IInputObjectTypeDescriptor<CreateArtigoRequest> descriptor)
        {
            descriptor.Description("Dados necessários para submeter um novo artigo ao ciclo editorial.");

            // Adiciona descrições de campo explícitas caso exista erro na tradução do DTO.
            descriptor.Field(f => f.Titulo).Description("Título do artigo.");
            descriptor.Field(f => f.Resumo).Description("Resumo do artigo.");
            descriptor.Field(f => f.Conteudo).Description("Conteúdo principal do artigo.");
            descriptor.Field(f => f.Tipo).Description("Tipo de artigo (e.g., Artigo, Blog, Entrevista).");

            // *** CAMPO ATUALIZADO ***
            descriptor.Field(f => f.Autores)
                .Type<NonNullType<ListType<NonNullType<AutorInputType>>>>()
                .Description("Lista de autores (ID, Nome, Url) cadastrados.");

            descriptor.Field(f => f.ReferenciasAutor).Description("Nomes de autores não cadastrados.");

            // *** CAMPO REMOVIDO ***
            // descriptor.Field(f => f.IdsAutor).Description("IDs dos usuários autores cadastrados.");
        }
    }
}