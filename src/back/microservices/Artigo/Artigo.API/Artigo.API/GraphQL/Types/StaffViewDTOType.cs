using Artigo.Server.DTOs;
using HotChocolate.Types;

namespace Artigo.API.GraphQL.Types
{
    /// <sumario>
    /// Mapeia o StaffViewDTO para um tipo de objeto GraphQL.
    /// Representa o 'Staff View Format' (informações de um membro da equipe).
    /// </sumario>
    public class StaffViewDTOType : ObjectType<StaffViewDTO>
    {
        protected override void Configure(IObjectTypeDescriptor<StaffViewDTO> descriptor)
        {
            descriptor.Description("Representa as informações de um membro da equipe (Staff).");

            descriptor.Field(f => f.UsuarioId).Type<NonNullType<IdType>>();
            descriptor.Field(f => f.Nome).Type<NonNullType<StringType>>();
            descriptor.Field(f => f.Url).Type<StringType>();
            descriptor.Field(f => f.Job).Type<NonNullType<EnumType<Artigo.Intf.Enums.FuncaoTrabalho>>>();
            descriptor.Field(f => f.IsActive).Type<NonNullType<BooleanType>>();
        }
    }
}