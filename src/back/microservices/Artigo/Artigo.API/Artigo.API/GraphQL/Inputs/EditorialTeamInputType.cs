using Artigo.Intf.Entities;
using HotChocolate.Types;
using System.Collections.Generic;

namespace Artigo.API.GraphQL.Inputs
{
    /// <sumario>
    /// Define o tipo de input no GraphQL para a entidade EditorialTeam.
    /// Usado pela mutação 'atualizarEquipeEditorial'.
    /// </sumario>
    public class EditorialTeamInputType : InputObjectType<EditorialTeam>
    {
        protected override void Configure(IInputObjectTypeDescriptor<EditorialTeam> descriptor)
        {
            descriptor.Description("Define a equipe editorial completa para um artigo. Todos os campos são de substituição total.");

            // Nota: Os campos devem ser anuláveis (?) na entidade, mas o input pode exigi-los.
            // Aqui, permitimos listas, mas o EditorId é obrigatório.

            descriptor.Field(f => f.InitialAuthorId)
                .Type<NonNullType<ListType<NonNullType<IdType>>>>()
                .Description("Lista completa de IDs de USUÁRIO dos autores.");

            descriptor.Field(f => f.EditorIds)
                .Type<NonNullType<ListType<NonNullType<IdType>>>>()
                .Description("Lista de IDs de STAFF dos Editores Chefes responsáveis.");

            descriptor.Field(f => f.ReviewerIds)
                .Type<NonNullType<ListType<NonNullType<IdType>>>>()
                .Description("Lista completa de IDs de USUÁRIO dos Revisores designados.");

            descriptor.Field(f => f.CorrectorIds)
                .Type<NonNullType<ListType<NonNullType<IdType>>>>()
                .Description("Lista completa de IDs de USUÁRIO dos Corretores designados.");
        }
    }
}