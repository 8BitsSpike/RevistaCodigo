using Artigo.Intf.Enums;
using Artigo.Intf.Inputs;
using HotChocolate.Types;

namespace Artigo.API.GraphQL.Inputs
{
    /// <sumario>
    /// Define o tipo de input no GraphQL para a atualização de um Staff.
    /// Mapeia para o UpdateStaffInput DTO do domínio.
    /// </sumario>
    public class UpdateStaffInputType : InputObjectType<UpdateStaffInput>
    {
        protected override void Configure(IInputObjectTypeDescriptor<UpdateStaffInput> descriptor)
        {
            descriptor.Description("Dados para atualizar um membro da Staff (promover, demover, aposentar). Campos nulos serão ignorados.");

            descriptor.Field(f => f.UsuarioId)
                .Type<NonNullType<IdType>>()
                .Description("O ID de Usuário (UsuarioId) do staff a ser atualizado.");

            descriptor.Field(f => f.Job)
                .Type<EnumType<FuncaoTrabalho>>()
                .Description("Opcional: A nova função de trabalho para o membro.");

            descriptor.Field(f => f.IsActive)
                .Type<BooleanType>()
                .Description("Opcional: O novo status de atividade (true para reinstaurar, false para aposentar).");
        }
    }
}