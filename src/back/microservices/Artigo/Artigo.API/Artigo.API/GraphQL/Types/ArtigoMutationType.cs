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

            // 1. CreateArtigoAsync -> criarArtigo
            descriptor.Field(f => f.CreateArtigoAsync(default!, default!))
                .Name("criarArtigo")
                .Argument("input", a => a.Type<NonNullType<InputObjectType<CreateArtigoRequest>>>())
                .Type<NonNullType<ArtigoType>>()
                .Description("Cria um novo artigo (Requer autenticação).");

            // 2. UpdateArtigoMetadataAsync -> atualizarMetadadosArtigo
            descriptor.Field(f => f.UpdateArtigoMetadataAsync(default!, default!, default!))
                .Name("atualizarMetadadosArtigo")
                .Argument("id", a => a.Type<NonNullType<IdType>>())
                .Argument("input", a => a.Type<NonNullType<InputObjectType<UpdateArtigoMetadataInput>>>())
                .Type<NonNullType<ArtigoType>>()
                .Description("Atualiza os metadados do artigo (Requer AuthZ/Staff).");

            // 3. CreatePublicCommentAsync -> criarComentarioPublico
            descriptor.Field(f => f.CreatePublicCommentAsync(default!, default!, default!, default!))
                .Name("criarComentarioPublico")
                .Type<NonNullType<InteractionType>>()
                .Argument("artigoId", a => a.Type<NonNullType<IdType>>())
                .Argument("content", a => a.Type<NonNullType<StringType>>())
                .Argument("parentCommentId", a => a.Type<IdType>())
                .Description("Cria um comentário público em um artigo.");

            // 4. CreateEditorialCommentAsync -> criarComentarioEditorial
            descriptor.Field(f => f.CreateEditorialCommentAsync(default!, default!, default!))
                .Name("criarComentarioEditorial")
                .Type<NonNullType<InteractionType>>()
                .Argument("artigoId", a => a.Type<NonNullType<IdType>>())
                .Argument("content", a => a.Type<NonNullType<StringType>>())
                .Description("Cria um comentário editorial interno (Requer AuthZ/Staff).");

            // 5. NOVO: CriarNovoStaffAsync -> criarNovoStaff
            descriptor.Field(f => f.CriarNovoStaffAsync(default!, default!))
                .Name("criarNovoStaff")
                .Argument("input", a => a.Type<NonNullType<InputObjectType<CreateStaffRequest>>>())
                .Type<NonNullType<StaffType>>() // Assume que StaffType está registrado e mapeado
                .Description("Promove um usuário externo a Staff e define sua função. (Requer Admin/EditorChefe).");
        }
    }
}