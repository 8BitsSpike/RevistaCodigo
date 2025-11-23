using Artigo.API.GraphQL.Inputs;
using Artigo.API.GraphQL.Mutations;
using Artigo.Server.DTOs;
using HotChocolate.Types;
using Artigo.Intf.Entities;
using Artigo.Intf.Inputs;
using System.Collections.Generic;

namespace Artigo.API.GraphQL.Types
{
    public class ArtigoMutationType : ObjectType<ArtigoMutation>
    {
        protected override void Configure(IObjectTypeDescriptor<ArtigoMutation> descriptor)
        {
            descriptor.Name("Mutation");
            descriptor.Field(f => f.CreateArtigoAsync(default!, default!, default!))
                .Name("criarArtigo")
                .Argument("input", a => a.Type<NonNullType<CreateArtigoInput>>())
                .Argument("commentary", a => a.Type<NonNullType<StringType>>());

            descriptor.Field(f => f.UpdateArtigoMetadataAsync(default!, default!, default!, default!))
                .Name("atualizarMetadadosArtigo")
                .Argument("id", a => a.Type<NonNullType<IdType>>())
                .Argument("input", a => a.Type<NonNullType<UpdateArtigoInput>>())
                .Argument("commentary", a => a.Type<NonNullType<StringType>>());

            descriptor.Field(f => f.AtualizarConteudoArtigoAsync(default!, default!, default!, default!, default!))
                .Name("atualizarConteudoArtigo")
                .Argument("artigoId", a => a.Type<NonNullType<IdType>>())
                .Argument("newContent", a => a.Type<NonNullType<StringType>>())
                .Argument("midias", a => a.Type<NonNullType<ListType<NonNullType<MidiaEntryInputType>>>>())
                .Argument("commentary", a => a.Type<NonNullType<StringType>>());

            descriptor.Field(f => f.AtualizarEquipeEditorialAsync(default!, default!, default!, default!))
                .Name("atualizarEquipeEditorial")
                .Argument("artigoId", a => a.Type<NonNullType<IdType>>())
                .Argument("teamInput", a => a.Type<NonNullType<EditorialTeamInputType>>())
                .Argument("commentary", a => a.Type<NonNullType<StringType>>());

            descriptor.Field(f => f.CreatePublicCommentAsync(default!, default!, default!, default!, default!))
                .Name("criarComentarioPublico")
                .Argument("artigoId", a => a.Type<NonNullType<IdType>>())
                .Argument("content", a => a.Type<NonNullType<StringType>>())
                .Argument("usuarioNome", a => a.Type<NonNullType<StringType>>())
                .Argument("parentCommentId", a => a.Type<IdType>());

            descriptor.Field(f => f.CreateEditorialCommentAsync(default!, default!, default!, default!))
                .Name("criarComentarioEditorial")
                .Argument("artigoId", a => a.Type<NonNullType<IdType>>())
                .Argument("content", a => a.Type<NonNullType<StringType>>())
                .Argument("usuarioNome", a => a.Type<NonNullType<StringType>>());

            descriptor.Field(f => f.AtualizarInteracaoAsync(default!, default!, default!, default!))
                .Name("atualizarInteracao")
                .Argument("interacaoId", a => a.Type<NonNullType<IdType>>())
                .Argument("newContent", a => a.Type<NonNullType<StringType>>())
                .Argument("commentary", a => a.Type<NonNullType<StringType>>());

            descriptor.Field(f => f.DeletarInteracaoAsync(default!, default!, default!))
                .Name("deletarInteracao")
                .Argument("interacaoId", a => a.Type<NonNullType<IdType>>())
                .Argument("commentary", a => a.Type<NonNullType<StringType>>());

            descriptor.Field(f => f.CriarNovoStaffAsync(default!, default!, default!))
                .Name("criarNovoStaff")
                .Argument("input", a => a.Type<NonNullType<CreateStaffInput>>())
                .Argument("commentary", a => a.Type<NonNullType<StringType>>());

            descriptor.Field(f => f.AtualizarStaffAsync(default!, default!, default!))
                .Name("atualizarStaff")
                .Argument("input", a => a.Type<NonNullType<UpdateStaffInputType>>())
                .Argument("commentary", a => a.Type<NonNullType<StringType>>());

            descriptor.Field(f => f.CriarVolumeAsync(default!, default!, default!))
                .Name("criarVolume")
                .Argument("input", a => a.Type<NonNullType<CreateVolumeInputType>>())
                .Argument("commentary", a => a.Type<NonNullType<StringType>>());

            descriptor.Field(f => f.AtualizarMetadadosVolumeAsync(default!, default!, default!, default!))
                .Name("atualizarMetadadosVolume")
                .Argument("volumeId", a => a.Type<NonNullType<IdType>>())
                .Argument("input", a => a.Type<NonNullType<UpdateVolumeMetadataInputType>>())
                .Argument("commentary", a => a.Type<NonNullType<StringType>>());

            descriptor.Field(f => f.AddStaffComentarioAsync(default!, default!, default!, default!))
                .Name("addStaffComentario")
                .Argument("historyId", a => a.Type<NonNullType<IdType>>())
                .Argument("comment", a => a.Type<NonNullType<StringType>>())
                .Argument("parent", a => a.Type<IdType>());

            descriptor.Field(f => f.UpdateStaffComentarioAsync(default!, default!, default!, default!))
                .Name("updateStaffComentario")
                .Argument("historyId", a => a.Type<NonNullType<IdType>>())
                .Argument("comentarioId", a => a.Type<NonNullType<IdType>>())
                .Argument("newContent", a => a.Type<NonNullType<StringType>>());

            descriptor.Field(f => f.DeleteStaffComentarioAsync(default!, default!, default!))
                .Name("deleteStaffComentario")
                .Argument("historyId", a => a.Type<NonNullType<IdType>>())
                .Argument("comentarioId", a => a.Type<NonNullType<IdType>>());

            descriptor.Field(f => f.CriarRequisicaoPendenteAsync(default!, default!))
                .Name("criarRequisicaoPendente")
                .Argument("input", a => a.Type<NonNullType<InputObjectType<Pending>>>());

            descriptor.Field(f => f.ResolverRequisicaoPendenteAsync(default!, default!, default!))
                .Name("resolverRequisicaoPendente")
                .Argument("pendingId", a => a.Type<NonNullType<IdType>>())
                .Argument("isApproved", a => a.Type<NonNullType<BooleanType>>());
        }
    }
}