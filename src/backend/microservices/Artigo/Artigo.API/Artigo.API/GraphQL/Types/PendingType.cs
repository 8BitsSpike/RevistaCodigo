using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using HotChocolate.Types;
using HotChocolate.Resolvers;
using System.Threading.Tasks;

namespace Artigo.API.GraphQL.Types
{
    /// <sumario>
    /// Mapeia a entidade Pending, representando uma requisição de fluxo de trabalho pendente de aprovação.
    /// </sumario>
    public class PendingType : ObjectType<Pending>
    {
        protected override void Configure(IObjectTypeDescriptor<Pending> descriptor)
        {
            descriptor.Description("Um item na fila de aprovação editorial (e.g., pedido para publicar, mudar função de staff).");

            descriptor.Field(f => f.Id).Type<NonNullType<IdType>>().Description("ID único da requisição pendente.");

            // Campos de Alvo
            descriptor.Field(f => f.TargetEntityId).Type<NonNullType<IdType>>().Description("ID da entidade alvo da ação (e.g., Artigo.Id, Staff.Id).");
            descriptor.Field(f => f.TargetType).Type<NonNullType<EnumType<TipoEntidadeAlvo>>>().Description("O tipo de entidade alvo."); 

            // Campos de Ação
            descriptor.Field(f => f.CommandParametersJson).Description("Parâmetros do comando JSON (dados complexos para execução).");
            descriptor.Field(f => f.CommandType).Description("O tipo de comando ou ação solicitada (e.g., Publicar, Rejeitar, MudarFuncao).");
            descriptor.Field(f => f.Commentary).Description("Comentário ou justificativa do solicitante.");

            // Metadados
            descriptor.Field(f => f.Status).Type<NonNullType<EnumType<StatusPendente>>>().Description("Status atual da requisição (AguardandoRevisao, Aprovado, Rejeitado)."); 
            descriptor.Field(f => f.RequesterUsuarioId).Type<NonNullType<IdType>>().Description("ID do usuário externo que criou esta requisição.");
            descriptor.Field(f => f.DateRequested).Description("Data e hora em que a requisição foi criada.");
        }
    }
}