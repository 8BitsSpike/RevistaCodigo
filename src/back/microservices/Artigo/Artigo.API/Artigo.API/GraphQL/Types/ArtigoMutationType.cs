using Artigo.API.GraphQL.Inputs;
using Artigo.API.GraphQL.Mutations;
using Artigo.Server.DTOs;
using HotChocolate.Types;
using Artigo.Intf.Entities;
using Artigo.Intf.Inputs;
using System.Collections.Generic; // Adicionado para List<>

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
                .Argument("input", a => a.Type<NonNullType<CreateArtigoInput>>())
                .Argument("commentary", a => a.Type<NonNullType<StringType>>().Description("Comentário ou justificativa para a mutação."))
                .Type<NonNullType<ArtigoType>>()
                .Description("Cria um novo artigo (Requer autenticação).");

            // 2. UpdateArtigoMetadataAsync -> atualizarMetadadosArtigo
            descriptor.Field(f => f.UpdateArtigoMetadataAsync(default!, default!, default!, default!))
                .Name("atualizarMetadadosArtigo")
                .Type<NonNullType<ArtigoType>>() // Retorna o ArtigoDTO atualizado
                .Argument("id", a => a.Type<NonNullType<IdType>>().Description("ID do artigo a ser atualizado."))
                .Argument("input", a => a.Type<NonNullType<UpdateArtigoInput>>().Description("Dados parciais para atualização."))
                .Argument("commentary", a => a.Type<NonNullType<StringType>>().Description("Comentário ou justificativa para a mutação."))
                .Description("Atualiza os metadados de um artigo (Requer AuthZ).");

            // 3. AtualizarConteudoArtigoAsync -> atualizarConteudoArtigo
            descriptor.Field(f => f.AtualizarConteudoArtigoAsync(default!, default!, default!, default!, default!))
                .Name("atualizarConteudoArtigo")
                .Type<NonNullType<BooleanType>>()
                .Argument("artigoId", a => a.Type<NonNullType<IdType>>().Description("ID do artigo a ser atualizado."))
                .Argument("newContent", a => a.Type<NonNullType<StringType>>().Description("O novo conteúdo principal do artigo."))
                .Argument("midias", a => a.Type<NonNullType<ListType<NonNullType<MidiaEntryInputType>>>>().Description("A nova lista completa de mídias para esta versão."))
                .Argument("commentary", a => a.Type<NonNullType<StringType>>().Description("Comentário ou justificativa para a mutação."))
                .Description("Atualiza o conteúdo e a lista de mídias de um artigo (Requer AuthZ).");

            // 4. AtualizarEquipeEditorialAsync -> atualizarEquipeEditorial
            descriptor.Field(f => f.AtualizarEquipeEditorialAsync(default!, default!, default!, default!))
                .Name("atualizarEquipeEditorial")
                .Type<NonNullType<EditorialType>>() // Retorna a entidade Editorial atualizada
                .Argument("artigoId", a => a.Type<NonNullType<IdType>>().Description("ID do artigo cuja equipe será atualizada."))
                .Argument("teamInput", a => a.Type<NonNullType<EditorialTeamInputType>>().Description("O novo objeto de equipe editorial."))
                .Argument("commentary", a => a.Type<NonNullType<StringType>>().Description("Comentário ou justificativa para a mutação."))
                .Description("Atualiza a equipe editorial (revisores, corretores) de um artigo. (Requer AuthZ de Staff).");


            // 5. CreatePublicCommentAsync -> criarComentarioPublico
            descriptor.Field(f => f.CreatePublicCommentAsync(default!, default!, default!, default!, default!))
                .Name("criarComentarioPublico")
                .Type<NonNullType<InteractionType>>()
                .Argument("artigoId", a => a.Type<NonNullType<IdType>>().Description("ID do artigo a ser comentado."))
                .Argument("content", a => a.Type<NonNullType<StringType>>().Description("O conteúdo do comentário."))
                .Argument("usuarioNome", a => a.Type<NonNullType<StringType>>().Description("Nome de exibição (denormalizado) do usuário."))
                .Argument("parentCommentId", a => a.Type<IdType>().Description("O ID do comentário pai (para respostas)."))
                .Description("Cria um novo comentário público em um artigo (Requer autenticação).");

            // 6. CreateEditorialCommentAsync -> criarComentarioEditorial
            descriptor.Field(f => f.CreateEditorialCommentAsync(default!, default!, default!, default!))
                .Name("criarComentarioEditorial")
                .Type<NonNullType<InteractionType>>()
                .Argument("artigoId", a => a.Type<NonNullType<IdType>>().Description("ID do artigo (em revisão) a ser comentado."))
                .Argument("content", a => a.Type<NonNullType<StringType>>().Description("O conteúdo do comentário."))
                .Argument("usuarioNome", a => a.Type<NonNullType<StringType>>().Description("Nome de exibição (denormalizado) do usuário."))
                .Description("Cria um novo comentário editorial em um artigo (Requer AuthZ de Staff/Autor).");

            // 7. CriarNovoStaffAsync -> criarNovoStaff
            descriptor.Field(f => f.CriarNovoStaffAsync(default!, default!, default!))
                .Name("criarNovoStaff")
                .Type<NonNullType<StaffType>>()
                .Argument("input", a => a.Type<NonNullType<CreateStaffInput>>().Description("Dados do novo membro Staff."))
                .Argument("commentary", a => a.Type<NonNullType<StringType>>().Description("Comentário ou justificativa para a mutação."))
                .Description("Cria um novo registro de Staff para um usuário (Requer AuthZ de Admin/EditorChefe).");

            // 8. CriarVolumeAsync -> criarVolume
            descriptor.Field(f => f.CriarVolumeAsync(default!, default!, default!))
                .Name("criarVolume")
                .Type<NonNullType<VolumeType>>()
                .Argument("input", a => a.Type<NonNullType<CreateVolumeInputType>>().Description("Dados do novo Volume (edição).")) // <-- TIPO ALTERADO
                .Argument("commentary", a => a.Type<NonNullType<StringType>>().Description("Comentário ou justificativa para a mutação."))
                .Description("Cria um novo Volume/Edição da revista (Requer AuthZ de Staff).");

            // 9. AtualizarMetadadosVolumeAsync -> atualizarMetadadosVolume
            descriptor.Field(f => f.AtualizarMetadadosVolumeAsync(default!, default!, default!, default!))
                .Name("atualizarMetadadosVolume")
                .Type<NonNullType<BooleanType>>()
                .Argument("volumeId", a => a.Type<NonNullType<IdType>>().Description("ID do volume a ser atualizado."))
                .Argument("input", a => a.Type<NonNullType<UpdateVolumeMetadataInputType>>().Description("Dados parciais para atualização."))
                .Argument("commentary", a => a.Type<NonNullType<StringType>>().Description("Comentário ou justificativa para a mutação."))
                .Description("Atualiza os metadados de um Volume (Requer AuthZ de Staff).");


            // =========================================================================
            // MUTAÇÕES DE STAFF (COMENTÁRIOS INTERNOS)
            // =========================================================================
            descriptor.Field(f => f.AddStaffComentarioAsync(default!, default!, default!, default!))
                .Name("addStaffComentario")
                .Type<NonNullType<ArtigoHistoryType>>()
                .Argument("historyId", a => a.Type<NonNullType<IdType>>().Description("ID do histórico a ser comentado."))
                .Argument("comment", a => a.Type<NonNullType<StringType>>().Description("O conteúdo do comentário."))
                .Argument("parent", a => a.Type<IdType>().Description("ID do comentário pai (para respostas)."))
                .Description("Adiciona um comentário interno a uma versão do histórico. (Requer AuthZ).");

            descriptor.Field(f => f.UpdateStaffComentarioAsync(default!, default!, default!, default!))
                .Name("updateStaffComentario")
                .Type<NonNullType<ArtigoHistoryType>>()
                .Argument("historyId", a => a.Type<NonNullType<IdType>>().Description("ID do histórico onde o comentário está."))
                .Argument("comentarioId", a => a.Type<NonNullType<IdType>>().Description("ID do comentário a ser atualizado."))
                .Argument("newContent", a => a.Type<NonNullType<StringType>>().Description("O novo conteúdo do comentário."))
                .Description("Atualiza um comentário interno. (Requer ser o autor).");

            descriptor.Field(f => f.DeleteStaffComentarioAsync(default!, default!, default!))
                .Name("deleteStaffComentario")
                .Type<NonNullType<ArtigoHistoryType>>()
                .Argument("historyId", a => a.Type<NonNullType<IdType>>().Description("ID do histórico onde o comentário está."))
                .Argument("comentarioId", a => a.Type<NonNullType<IdType>>().Description("ID do comentário a ser deletado."))
                .Description("Deleta um comentário interno. (Requer ser o autor ou Admin/Chefe).");

            // =========================================================================
            // MUTAÇÕES DE INTERAÇÃO (COMENTÁRIOS PÚBLICOS/EDITORIAIS)
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
            // *** MUTAÇÃO (Pending Manual) ***
            // =========================================================================

            descriptor.Field(f => f.CriarRequisicaoPendenteAsync(default!, default!))
                .Name("criarRequisicaoPendente")
                .Type<NonNullType<PendingType>>()
                .Argument("input", a => a.Type<NonNullType<InputObjectType<Pending>>>().Description("O objeto Pending a ser criado."))
                .Description("Cria manualmente uma requisição pendente. (Requer Admin).");
        }
    }
}