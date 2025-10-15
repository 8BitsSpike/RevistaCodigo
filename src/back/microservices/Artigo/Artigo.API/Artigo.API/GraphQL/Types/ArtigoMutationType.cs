using Artigo.API.GraphQL.Inputs;
using Artigo.API.GraphQL.Mutations;
using Artigo.Server.DTOs;
using HotChocolate.Types;

namespace Artigo.API.GraphQL.Types
{
    /// <sumario>
    /// Define manualmente o tipo Mutation para Artigo, corrigindo o erro de reflexão.
    /// </sumario>
    public class ArtigoMutationType : ObjectType<ArtigoMutation>
    {
        protected override void Configure(IObjectTypeDescriptor<ArtigoMutation> descriptor)
        {
            // Define o nome do tipo raiz (Obrigatório)
            descriptor.Name("Mutation");

            // 1. CreateArtigoAsync
            descriptor.Field(f => f.CreateArtigoAsync(default!, default!))
                .Name("createArtigo")
                // FIX 1: Corrected C# Class Name: CreateArtigoRequest
                // FIX 2: Used InputObjectType<T> helper to register the C# class as a GraphQL Input type
                .Argument("input", a => a.Type<NonNullType<InputObjectType<CreateArtigoRequest>>>())
                .Type<NonNullType<ArtigoType>>()
                .Description("Cria um novo artigo (Requer autenticação).");

            // 2. UpdateArtigoMetadataAsync
            descriptor.Field(f => f.UpdateArtigoMetadataAsync(default!, default!, default!))
                .Name("updateArtigoMetadata")
                .Argument("id", a => a.Type<NonNullType<IdType>>())
                // FIX: Used InputObjectType<T> helper to register the C# class as a GraphQL Input type
                .Argument("input", a => a.Type<NonNullType<InputObjectType<UpdateArtigoMetadataInput>>>())
                .Type<NonNullType<ArtigoType>>()
                .Description("Atualiza os metadados do artigo (Requer AuthZ/Staff).");

            // 3. CreatePublicCommentAsync
            descriptor.Field(f => f.CreatePublicCommentAsync(default!, default!, default!, default!))
                .Name("createPublicComment")
                .Type<NonNullType<InteractionType>>()
                .Argument("artigoId", a => a.Type<NonNullType<IdType>>())
                .Argument("content", a => a.Type<NonNullType<StringType>>())
                .Argument("parentCommentId", a => a.Type<IdType>())
                .Description("Cria um comentário público em um artigo.");

            // 4. CreateEditorialCommentAsync
            descriptor.Field(f => f.CreateEditorialCommentAsync(default!, default!, default!))
                .Name("createEditorialComment")
                .Type<NonNullType<InteractionType>>()
                .Argument("artigoId", a => a.Type<NonNullType<IdType>>())
                .Argument("content", a => a.Type<NonNullType<StringType>>())
                .Description("Cria um comentário editorial interno (Requer AuthZ/Staff).");
        }
    }
}
