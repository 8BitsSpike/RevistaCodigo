using Artigo.Server.DTOs;
using HotChocolate.Types;

namespace Artigo.API.GraphQL.Inputs
{
    /// <sumario>
    /// Define o tipo de input no GraphQL para a criação de um novo Staff.
    /// Mapeia para o DTO CreateStaffRequest.
    /// </sumario>
    public class CreateStaffInput : InputObjectType<CreateStaffRequest>
    {
        protected override void Configure(IInputObjectTypeDescriptor<CreateStaffRequest> descriptor)
        {
            descriptor.Description("Dados necessários para promover um usuário a membro da equipe Staff.");

            descriptor.Field(f => f.UsuarioId).Type<NonNullType<IdType>>().Description("ID do usuário no sistema externo.");
            descriptor.Field(f => f.Job).Type<NonNullType<EnumType<Artigo.Intf.Enums.FuncaoTrabalho>>>().Description("Função de trabalho a ser atribuída.");
            descriptor.Field(f => f.Nome).Type<NonNullType<StringType>>().Description("Nome de exibição do usuário (para denormalização).");
            descriptor.Field(f => f.Url).Type<StringType>().Description("URL da foto de perfil do usuário (para denormalização).");
        }
    }
}