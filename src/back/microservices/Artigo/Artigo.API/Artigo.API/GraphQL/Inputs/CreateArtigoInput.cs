using Artigo.Server.DTOs;
using HotChocolate.Types;

namespace Artigo.API.GraphQL.Inputs
{
    /// <sumario>
    /// Define o tipo de input GraphQL para a criação de um novo Artigo.
    /// Mapeia diretamente para o DTO de requisição da camada de aplicação.
    /// </sumario>
    public class CreateArtigoInput : InputObjectType<CreateArtigoRequest>
    {
        protected override void Configure(IInputObjectTypeDescriptor<CreateArtigoRequest> descriptor)
        {
            descriptor.Description("Dados necessários para submeter um novo artigo ao ciclo editorial.");
        }
    }
}