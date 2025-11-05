using Artigo.Server.DTOs;
using HotChocolate.Types;

namespace Artigo.API.GraphQL.Inputs
{
    /// <sumario>
    /// Define o tipo de input no GraphQL para os dados denormalizados de um Autor.
    /// Usado pela mutação 'criarArtigo'.
    /// </sumario>
    public class AutorInputType : InputObjectType<AutorInputDTO>
    {
        protected override void Configure(IInputObjectTypeDescriptor<AutorInputDTO> descriptor)
        {
            descriptor.Description("Dados denormalizados de um autor (ID, Nome, Url) necessários para criar um artigo.");

            descriptor.Field(f => f.UsuarioId).Type<NonNullType<IdType>>().Description("ID do usuário no sistema externo.");
            descriptor.Field(f => f.Nome).Type<NonNullType<StringType>>().Description("Nome de exibição do autor.");
            descriptor.Field(f => f.Url).Type<StringType>().Description("URL da foto de perfil do autor.");
        }
    }
}