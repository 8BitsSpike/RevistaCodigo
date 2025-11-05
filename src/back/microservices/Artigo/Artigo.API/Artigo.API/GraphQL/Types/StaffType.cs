using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using Artigo.Server.DTOs;
// Removido: using Artigo.API.GraphQL.DataLoaders;
using HotChocolate.Types;
using HotChocolate.Resolvers;
using System.Threading.Tasks;
using System.Threading;

namespace Artigo.API.GraphQL.Types
{
    /// <sumario>
    /// Resolver para buscar dados externos do UsuarioAPI (Parent: Staff).
    /// </sumario>
    // *** CLASSE REMOVIDA ***
    // public class StaffUserResolver
    // {
    //    ...
    // }

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

            // *** NOVOS CAMPOS (DENORMALIZADOS) ***
            descriptor.Field(f => f.Nome)
                .Type<NonNullType<StringType>>()
                .Description("Nome de exibição do staff (denormalizado).");

            descriptor.Field(f => f.Url)
                .Type<StringType>()
                .Description("URL da foto de perfil do staff (denormalizado).");

            descriptor.Field(f => f.Job).Type<NonNullType<EnumType<FuncaoTrabalho>>>().Description("A função principal do membro da equipe (e.g., EditorChefe).");
            descriptor.Field(f => f.IsActive).Description("Indicador de status: se o membro da Staff está ativo.");

            // *** CAMPO REMOVIDO ***
            // descriptor.Field<StaffUserResolver>(r => r.GetExternalUserAsync(default!, default!, default!))
            //    .Name("usuarioInfo")
            //    ...
        }
    }
}