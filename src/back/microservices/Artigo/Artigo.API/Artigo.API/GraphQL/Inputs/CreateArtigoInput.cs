using Artigo.Server.DTOs;
using HotChocolate.Types;

namespace Artigo.API.GraphQL.Inputs
{
    /// <sumario>
    /// Define o tipo de input GraphQL para a criação de um novo Artigo.
    /// Mapeia diretamente para o DTO de requisição da camada de aplicação.
    /// NOTA: Os nomes dos campos (e.g., 'titulo', 'conteudo') são inferidos
    /// automaticamente do DTO C# 'CreateArtigoRequest'.
    /// </sumario>
    public class CreateArtigoInput : InputObjectType<CreateArtigoRequest>
    {
        protected override void Configure(IInputObjectTypeDescriptor<CreateArtigoRequest> descriptor)
        {
            descriptor.Description("Dados necessários para submeter um novo artigo ao ciclo editorial.");

            // Adiciona descrições de campo explícitas se necessário, ou confiar na tradução do DTO.
            descriptor.Field(f => f.Titulo).Description("Título do artigo.");
            descriptor.Field(f => f.Resumo).Description("Resumo do artigo.");
            descriptor.Field(f => f.Conteudo).Description("Conteúdo principal do artigo.");
            descriptor.Field(f => f.Tipo).Description("Tipo de artigo (e.g., Artigo, Blog, Entrevista).");
            descriptor.Field(f => f.IdsAutor).Description("IDs dos usuários autores cadastrados.");
            descriptor.Field(f => f.ReferenciasAutor).Description("Nomes de autores não cadastrados.");
        }
    }
}