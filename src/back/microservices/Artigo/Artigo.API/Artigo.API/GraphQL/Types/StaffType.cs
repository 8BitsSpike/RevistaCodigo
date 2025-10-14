using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using Artigo.Server.DTOs; // Necessary for ExternalUserDTO
using Artigo.API.GraphQL.DataLoaders; // Necessary for ExternalUserDataLoader
using HotChocolate.Types;
using HotChocolate.Resolvers;
using System.Threading.Tasks;
using System.Threading;

namespace Artigo.API.GraphQL.Types
{
    // NOTE: ExternalUserType is defined in AutorType.cs, so we reuse it.

    /// <sumario>
    /// Resolver para buscar dados externos do UsuarioAPI (Parent: Staff).
    /// </sumario>
    public class StaffUserResolver
    {
        public Task<ExternalUserDTO> GetExternalUserAsync(
            [Parent] Staff staff,
            ExternalUserDataLoader dataLoader,
            CancellationToken cancellationToken)
        {
            // A chave de busca é o UsuarioId externo. O ! asserts non-null return based on schema.
            return dataLoader.LoadAsync(staff.UsuarioId, cancellationToken)!;
        }
    }

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

            // FIX: Campo para resolver o nome e media (avatar) do Staff a partir do UsuarioAPI
            descriptor.Field<StaffUserResolver>(r => r.GetExternalUserAsync(default!, default!, default!))
                .Name("usuarioInfo")
                .Type<ExternalUserType>()
                .Description("Informações de perfil (nome, media) do Staff, buscadas no UsuarioAPI.");
        }
    }
}