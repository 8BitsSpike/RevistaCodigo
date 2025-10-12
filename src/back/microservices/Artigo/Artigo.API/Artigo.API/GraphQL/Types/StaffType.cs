using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using HotChocolate.Types;
using HotChocolate.Resolvers;
using System.Threading.Tasks;

namespace Artigo.API.GraphQL.Types
{
    /// <sumario>
    /// Mapeia a entidade Staff, representando o registro local de um membro da equipe editorial e sua função.
    /// </sumario>
    public class StaffType : ObjectType<Staff>
    {
        protected override void Configure(IObjectTypeDescriptor<Staff> descriptor)
        {
            descriptor.Description("Representa um membro da equipe editorial (Staff) e sua função de trabalho (JobRole).");

            descriptor.Field(f => f.Id).Type<NonNullType<IdType>>().Description("ID local do registro de Staff.");
            descriptor.Field(f => f.UsuarioId).Type<NonNullType<IdType>>().Description("ID do usuário no sistema externo (UsuarioApi).");
            descriptor.Field(f => f.Job).Type<NonNullType<EnumType<JobRole>>>().Description("A função principal do membro da equipe (e.g., EditorChefe).");
            descriptor.Field(f => f.IsActive).Description("Indicador de status: se o membro da Staff está ativo.");

            // TODO: Adicionar um resolver para carregar o nome e media (avatar) do Staff a partir do UsuarioAPI
            // Isto permitiria que o cliente buscasse Staff.usuarioInfo.nome
        }
    }
}