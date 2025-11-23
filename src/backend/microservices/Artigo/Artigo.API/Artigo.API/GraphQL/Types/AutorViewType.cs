using Artigo.Server.DTOs;
using HotChocolate.Types;

namespace Artigo.API.GraphQL.Types
{
    /// <sumario>
    /// Mapeia o AutorViewDTO para um tipo de objeto GraphQL.
    /// Representa o 'Autor Format' (informações públicas de um autor).
    /// </sumario>
    public class AutorViewType : ObjectType<AutorViewDTO>
    {
        protected override void Configure(IObjectTypeDescriptor<AutorViewDTO> descriptor)
        {
            descriptor.Description("Representa as informações públicas de um autor (ID, Nome, Url).");

            descriptor.Field(f => f.UsuarioId).Type<NonNullType<IdType>>();
            descriptor.Field(f => f.Nome).Type<NonNullType<StringType>>();
            descriptor.Field(f => f.Url).Type<StringType>();
        }
    }
}