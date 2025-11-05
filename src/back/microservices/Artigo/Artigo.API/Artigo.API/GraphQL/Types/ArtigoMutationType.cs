using Artigo.API.GraphQL.Inputs;
using Artigo.API.GraphQL.Mutations;
using Artigo.Server.DTOs;
using HotChocolate.Types;
using Artigo.Intf.Entities;
using Artigo.Intf.Inputs; // *** ADICIONADO ***

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
            descriptor.Field(f => f.CreateArtigoAsync(default!, default!, default!))
                .Name("criarArtigo")
                .Argument("input", a => a.Type<NonNullType<CreateArtigoInput>>()) // *** ATUALIZADO ***
                .Argument("commentary", a => a.Type<NonNullType<StringType>>().Description("Comentário ou justificativa para a mutação.")) // *** NOVO ***
                .Type<NonNullType<ArtigoType>>()
                .Description("Cria um novo artigo (Requer autenticação).");

            // 2. UpdateArtigoMetadataAsync -> atualizarMetadadosArtigo
            descriptor.Field(f => f.UpdateArtigoMetadataAsync(default!, default!, default!, default!))
                .Name("atualizarMetadadosArtigo")
                .Argument("id", a => a.Type<NonNullType<IdType>>())
                .Argument("input", a => a.Type<NonNullType<UpdateArtigoInput>>()) // *** ATUALIZADO ***
                .Argument("commentary", a => a.Type<NonNullType<StringType>>().Description("Comentário ou justificativa para a mutação.")) // *** NOVO ***
                .Type<NonNullType<ArtigoType>>()
                .Description("Atualiza os metadados do artigo (Requer AuthZ/Staff).");

            // 3. CreatePublicCommentAsync -> criarComentarioPublico
            descriptor.Field(f => f.CreatePublicCommentAsync(default!, default!, default!, default!, default!))
                .Name("criarComentarioPublico")
                .Type<NonNullType<InteractionType>>()
                .Argument("artigoId", a => a.Type<NonNullType<IdType>>())
                .Argument("content", a => a.Type<NonNullType<StringType>>())
                .Argument("usuarioNome", a => a.Type<NonNullType<StringType>>().Description("Nome de exibição do usuário que está comentando.")) // *** NOVO ***
                .Argument("parentCommentId", a => a.Type<IdType>())
                .Description("Cria um comentário público em um artigo.");

            // 4. CreateEditorialCommentAsync -> criarComentarioEditorial
            descriptor.Field(f => f.CreateEditorialCommentAsync(default!, default!, default!, default!))
                .Name("criarComentarioEditorial")
                .Type<NonNullType<InteractionType>>()
                .Argument("artigoId", a => a.Type<NonNullType<IdType>>())
                .Argument("content", a => a.Type<NonNullType<StringType>>())
                .Argument("usuarioNome", a => a.Type<NonNullType<StringType>>().Description("Nome de exibição do usuário (Staff) que está comentando.")) // *** NOVO ***
                .Description("Cria um comentário editorial (pós-publicação) (Requer AuthZ/Staff).");

            // 5. CriarNovoStaffAsync -> criarNovoStaff
            descriptor.Field(f => f.CriarNovoStaffAsync(default!, default!, default!))
                .Name("criarNovoStaff")
                .Argument("input", a => a.Type<NonNullType<CreateStaffInput>>()) // *** ATUALIZADO ***
                .Argument("commentary", a => a.Type<NonNullType<StringType>>().Description("Comentário ou justificativa para a mutação.")) // *** NOVO ***
                .Type<NonNullType<StaffType>>()
                .Description("Promove um usuário externo a Staff e define sua função. (Requer Admin/EditorChefe).");

            // 6. CriarVolumeAsync -> criarVolume
            descriptor.Field(f => f.CriarVolumeAsync(default!, default!, default!))
                .Name("criarVolume")
                .Argument("input", a => a.Type<NonNullType<InputObjectType<CreateVolumeRequest>>>())
                .Argument("commentary", a => a.Type<NonNullType<StringType>>().Description("Comentário ou justificativa para a mutação.")) // *** NOVO ***
                .Type<NonNullType<VolumeType>>()
                .Description("Cria um novo Volume (edição de revista). (Requer AuthZ/Staff).");

            // =========================================================================
            // *** NOVAS MUTAÇÕES (StaffComentario) ***
            // =========================================================================

            descriptor.Field(f => f.AddStaffComentarioAsync(default!, default!, default!, default!))
                .Name("adicionarStaffComentario")
                .Type<NonNullType<ArtigoHistoryType>>()
                .Argument("historyId", a => a.Type<NonNullType<IdType>>().Description("ID do registro de ArtigoHistory."))
                .Argument("comment", a => a.Type<NonNullType<StringType>>().Description("Conteúdo do comentário."))
                .Argument("parent", a => a.Type<IdType>().Description("ID do comentário 'pai' (para threading)."))
                .Description("Adiciona um comentário editorial interno a uma versão do histórico (Requer AuthZ/Staff).");

            descriptor.Field(f => f.UpdateStaffComentarioAsync(default!, default!, default!, default!))
                .Name("atualizarStaffComentario")
                .Type<NonNullType<ArtigoHistoryType>>()
                .Argument("historyId", a => a.Type<NonNullType<IdType>>().Description("ID do registro de ArtigoHistory."))
                .Argument("comentarioId", a => a.Type<NonNullType<IdType>>().Description("ID do comentário a ser atualizado."))
                .Argument("newContent", a => a.Type<NonNullType<StringType>>().Description("O novo conteúdo do comentário."))
                .Description("Atualiza um comentário editorial interno (Requer AuthZ/Staff).");

            descriptor.Field(f => f.DeleteStaffComentarioAsync(default!, default!, default!))
                .Name("deletarStaffComentario")
                .Type<NonNullType<ArtigoHistoryType>>()
                .Argument("historyId", a => a.Type<NonNullType<IdType>>().Description("ID do registro de ArtigoHistory."))
                .Argument("comentarioId", a => a.Type<NonNullType<IdType>>().Description("ID do comentário a ser deletado."))
                .Description("Deleta um comentário editorial interno (Requer AuthZ/Staff).");

            // =========================================================================
            // *** NOVAS MUTAÇÕES (Interaction) ***
            // =========================================================================

            descriptor.Field(f => f.AtualizarInteracaoAsync(default!, default!, default!, default!))
                .Name("atualizarInteracao")
                .Type<NonNullType<InteractionType>>()
                .Argument("interacaoId", a => a.Type<NonNullType<IdType>>().Description("ID do comentário a ser atualizado."))
                .Argument("newContent", a => a.Type<NonNullType<StringType>>().Description("O novo conteúdo do comentário."))
                .Argument("commentary", a => a.Type<NonNullType<StringType>>().Description("Comentário ou justificativa para a mutação (usado para logs)."))
                .Description("Atualiza um comentário. (Requer ser o autor).");

            descriptor.Field(f => f.DeletarInteracaoAsync(default!, default!, default!))
                .Name("deletarInteracao")
                .Type<NonNullType<BooleanType>>()
                .Argument("interacaoId", a => a.Type<NonNullType<IdType>>().Description("ID do comentário a ser deletado."))
                .Argument("commentary", a => a.Type<NonNullType<StringType>>().Description("Comentário ou justificativa para a mutação."))
                .Description("Deleta um comentário. (Requer ser o autor ou Staff).");

            // =========================================================================
            // *** NOVA MUTAÇÃO (Pending Manual) ***
            // =========================================================================

            descriptor.Field(f => f.CriarRequisicaoPendenteAsync(default!, default!))
                .Name("criarRequisicaoPendente")
                .Type<NonNullType<PendingType>>()
                .Argument("input", a => a.Type<NonNullType<InputObjectType<Pending>>>().Description("O objeto Pending a ser criado."))
                .Description("Cria manualmente uma requisição pendente. (Requer Admin).");
        }
    }
}