using System.Collections.Generic;
using System.Threading.Tasks;
using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using System;

namespace Artigo.Intf.Interfaces
{
    /// <sumario>
    /// Define o contrato para a logica de negocio da entidade Artigo.
    /// É responsavel por orquestrar repositorios e aplicar regras de negocio (e.g., validacao, autorizacao).
    /// </sumario>
    public interface IArtigoService
    {
        // =========================================================================
        // ARTIGO CORE MANAGEMENT (CRUD & STATUS)
        // =========================================================================

        /// <sumario>
        /// Retorna um Artigo para leitura publica.
        /// REGRA: Qualquer usuario pode ler se o Status for 'Published'.
        /// </sumario>
        /// <param name="id">O ID do artigo.</param>
        Task<Artigo.Intf.Entities.Artigo?> GetPublishedArtigoAsync(string id);

        /// <sumario>
        /// Retorna um Artigo para o ciclo editorial (unidade de trabalho).
        /// REGRA: Apenas Staff/Autores listados no artigo podem ler.
        /// </sumario>
        /// <param name="id">O ID do artigo.</param>
        /// <param name="currentUsuarioId">O ID do usuario externo tentando acessar.</param>
        Task<Artigo.Intf.Entities.Artigo?> GetArtigoForEditorialAsync(string id, string currentUsuarioId);

        /// <sumario>
        /// Cria um novo artigo e a entrada Editorial correspondente.
        /// REGRA: Qualquer usuario pode enviar um novo artigo.
        /// </sumario>
        /// <param name="artigo">O objeto Artigo preenchido.</param>
        /// <param name="usuarioId">O ID do usuario externo que esta criando o artigo (AutorPrincipal).</param>
        Task<Artigo.Intf.Entities.Artigo> CreateArtigoAsync(Artigo.Intf.Entities.Artigo artigo, string usuarioId);

        /// <sumario>
        /// Atualiza metadados simples (titulo, resumo) de um artigo.
        /// REGRA: Apenas Staff/Autores listados no artigo podem editar.
        /// </sumario>
        /// <param name="artigo">Artigo com dados para atualizacao.</param>
        /// <param name="currentUsuarioId">O ID do usuario externo tentando atualizar.</param>
        Task<bool> UpdateArtigoMetadataAsync(Artigo.Intf.Entities.Artigo artigo, string currentUsuarioId);

        /// <sumario>
        /// Atualiza o corpo do artigo, criando um novo registro em ArtigoHistory.
        /// REGRA: Apenas Staff/Autores listados no artigo podem editar.
        /// </sumario>
        /// <param name="artigoId">ID do artigo.</param>
        /// <param name="newContent">O novo corpo do texto do artigo.</param>
        /// <param name="currentUsuarioId">O ID do usuario externo tentando editar.</param>
        Task<bool> UpdateArtigoContentAsync(string artigoId, string newContent, string currentUsuarioId);

        /// <sumario>
        /// Altera o Status (Draft, InReview, Published, etc.) do artigo.
        /// REGRA: Apenas EditorBolsista e EditorChefe podem modificar o ArtigoStatus.
        /// </sumario>
        /// <param name="artigoId">ID do artigo.</param>
        /// <param name="newStatus">O novo status a ser definido.</param>
        /// <param name="currentUsuarioId">O ID do usuario externo solicitando a mudanca.</param>
        Task<bool> ChangeArtigoStatusAsync(string artigoId, ArtigoStatus newStatus, string currentUsuarioId);

        /// <sumario>
        /// Retorna todos os artigos que estao em um status editorial especifico.
        /// REGRA: Requer permissao de Staff.
        /// </sumario>
        /// <param name="status">O status editorial a ser filtrado.</param>
        Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> GetArtigosByStatusAsync(ArtigoStatus status, string currentUsuarioId);

        // =========================================================================
        // INTERACTION (COMENTARIOS) MANAGEMENT
        // =========================================================================

        /// <sumario>
        /// Cria um novo ComentarioPublico em um artigo publicado.
        /// REGRA: Qualquer usuario pode criar um ComentarioPublico em artigo Published.
        /// REGRA: Pode-se comentar em Comentarios Publicos (threading).
        /// </sumario>
        /// <param name="artigoId">ID do artigo.</param>
        /// <param name="newComment">O objeto Interaction preenchido (Content, UsuarioId).</param>
        /// <param name="parentCommentId">ID do comentario pai (threading). Deve ser um ComentarioPublico ou null.</param>
        Task<Interaction> CreatePublicCommentAsync(string artigoId, Interaction newComment, string? parentCommentId);

        /// <sumario>
        /// Cria um ComentarioEditorial em um artigo.
        /// REGRA: Apenas usuarios relacionados ao EditorialTeam podem comentar.
        /// REGRA: Ninguem pode comentar em um ComentarioEditorial (parentCommentId deve ser nulo).
        /// </sumario>
        /// <param name="artigoId">ID do artigo.</param>
        /// <param name="newComment">O objeto Interaction preenchido (Content, UsuarioId).</param>
        /// <param name="currentUsuarioId">O ID do usuario externo tentando comentar.</param>
        Task<Interaction> CreateEditorialCommentAsync(string artigoId, Interaction newComment, string currentUsuarioId);

        // =========================================================================
        // PENDING (FLUXO DE APROVACAO) MANAGEMENT
        // =========================================================================

        /// <sumario>
        /// Cria uma nova requisicao pendente (Pedido) no sistema.
        /// REGRA: Apenas EditorBolsistas podem criar novos itens Pendentes.
        /// </sumario>
        /// <param name="newRequest">O objeto Pending a ser criado.</param>
        /// <param name="currentUsuarioId">O ID do usuario externo solicitando.</param>
        Task<Pending> CreatePendingRequestAsync(Pending newRequest, string currentUsuarioId);

        /// <sumario>
        /// Modifica o Status de um item Pendente (Aprovado/Rejeitado).
        /// REGRA: Apenas EditorChefes e Administradores podem modificar PendingStatus.
        /// </sumario>
        /// <param name="pendingId">ID da requisicao pendente.</param>
        /// <param name="isApproved">Indica se a requisicao foi aprovada (true) ou rejeitada (false).</param>
        /// <param name="currentUsuarioId">O ID do usuario externo tentando resolver.</param>
        Task<bool> ResolvePendingRequestAsync(string pendingId, bool isApproved, string currentUsuarioId);

        // =========================================================================
        // VOLUME MANAGEMENT
        // =========================================================================

        /// <sumario>
        /// Atualiza os metadados e o indice de artigos de uma edicao (Volume).
        /// REGRA: Apenas usuarios na lista Staff podem editar.
        /// </sumario>
        /// <param name="updatedVolume">O objeto Volume com os dados atualizados.</param>
        /// <param name="currentUsuarioId">O ID do usuario externo tentando atualizar.</param>
        Task<bool> UpdateVolumeMetadataAsync(Volume updatedVolume, string currentUsuarioId);
    }
}
