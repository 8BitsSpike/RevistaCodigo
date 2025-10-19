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
        Task<Artigo.Intf.Entities.Artigo?> ObterArtigoPublicadoAsync(string id);

        /// <sumario>
        /// Retorna todos os artigos publicados para leitores não autenticados (visitantes).
        /// REGRA: Retorna apenas Artigos com Status 'Published'.
        /// </sumario>
        Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> ObterArtigosPublicadosParaVisitantesAsync();

        /// <sumario>
        /// Retorna um Artigo para o ciclo editorial (unidade de trabalho).
        /// REGRA: Apenas Staff/Autores listados no artigo podem ler.
        /// </sumario>
        /// <param name="id">O ID do artigo.</param>
        /// <param name="currentUsuarioId">O ID do usuario externo tentando acessar.</param>
        Task<Artigo.Intf.Entities.Artigo?> ObterArtigoParaEditorialAsync(string id, string currentUsuarioId);

        /// <sumario>
        /// Cria um novo artigo e a entrada Editorial correspondente.
        /// REGRA: Qualquer usuario pode enviar um novo artigo.
        /// </sumario>
        /// <param name="artigo">O objeto Artigo preenchido.</param>
        /// <param name="conteudoInicial">O conteúdo inicial do corpo do artigo (necessário para o ArtigoHistory).</param> 
        /// <param name="usuarioId">O ID do usuario externo que esta criando o artigo (AutorPrincipal).</param>
        Task<Artigo.Intf.Entities.Artigo> CreateArtigoAsync(Artigo.Intf.Entities.Artigo artigo, string conteudoInicial, string usuarioId);

        /// <sumario>
        /// Atualiza metadados simples (titulo, resumo) de um artigo.
        /// REGRA: Apenas Staff/Autores listados no artigo podem editar.
        /// </sumario>
        /// <param name="artigo">Artigo com dados para atualizacao.</param>
        /// <param name="currentUsuarioId">O ID do usuario externo tentando atualizar.</param>
        Task<bool> AtualizarMetadadosArtigoAsync(Artigo.Intf.Entities.Artigo artigo, string currentUsuarioId);

        /// <sumario>
        /// Atualiza o corpo do artigo, criando um novo registro em ArtigoHistory.
        /// REGRA: Apenas Staff/Autores listados no artigo podem editar.
        /// </summary>
        /// <param name="artigoId">ID do artigo.</param>
        /// <param name="newContent">O novo corpo do texto do artigo.</param>
        /// <param name="currentUsuarioId">O ID do usuario externo tentando editar.</param>
        Task<bool> AtualizarConteudoArtigoAsync(string artigoId, string newContent, string currentUsuarioId);

        /// <sumario>
        /// Altera o Status (Draft, InReview, Published, etc.) do artigo.
        /// REGRA: Apenas EditorBolsista e EditorChefe podem modificar o ArtigoStatus.
        /// </summary>
        /// <param name="artigoId">ID do artigo.</param>
        /// <param name="newStatus">O novo status a ser definido.</param>
        /// <param name="currentUsuarioId">O ID do usuario externo solicitando a mudanca.</param>
        Task<bool> AlterarStatusArtigoAsync(string artigoId, StatusArtigo newStatus, string currentUsuarioId); // FIX: ArtigoStatus -> StatusArtigo

        /// <sumario>
        /// Retorna todos os artigos que estao em um status editorial especifico.
        /// REGRA: Requer permissao de Staff.
        /// </summary>
        /// <param name="status">O status editorial a ser filtrado.</param>
        Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> ObterArtigosPorStatusAsync(StatusArtigo status, string currentUsuarioId); // FIX: ArtigoStatus -> StatusArtigo

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
        Task<Interaction> CriarComentarioPublicoAsync(string artigoId, Interaction newComment, string? parentCommentId);

        /// <sumario>
        /// Cria um ComentarioEditorial em um artigo.
        /// REGRA: Apenas usuarios relacionados ao EditorialTeam podem comentar.
        /// REGRA: Ninguem pode comentar em um ComentarioEditorial (parentCommentId deve ser nulo).
        /// </summary>
        /// <param name="artigoId">ID do artigo.</param>
        /// <param name="newComment">O objeto Interaction preenchido (Content, UsuarioId).</param>
        /// <param name="currentUsuarioId">O ID do usuario externo tentando comentar.</param>
        Task<Interaction> CriarComentarioEditorialAsync(string artigoId, Interaction newComment, string currentUsuarioId);

        // =========================================================================
        // PENDING (FLUXO DE APROVACAO) MANAGEMENT
        // =========================================================================

        /// <sumario>
        /// Cria uma nova requisicao pendente (Pedido) no sistema.
        /// REGRA: Apenas EditorBolsistas podem criar novos itens Pendentes.
        /// </summary>
        /// <param name="newRequest">O objeto Pending a ser criado.</param>
        /// <param name="currentUsuarioId">O ID do usuario externo solicitando.</param>
        Task<Pending> CriarRequisicaoPendenteAsync(Pending newRequest, string currentUsuarioId);

        /// <sumario>
        /// Modifica o Status de um item Pendente (Aprovado/Rejeitado).
        /// REGRA: Apenas EditorChefes e Administradores podem modificar PendingStatus.
        /// </summary>
        /// <param name="pendingId">ID da requisicao pendente.</param>
        /// <param name="isApproved">Indica se a requisicao foi aprovada (true) ou rejeitada (false).</param>
        /// <param name="currentUsuarioId">O ID do usuario externo tentando resolver.</param>
        Task<bool> ResolverRequisicaoPendenteAsync(string pendingId, bool isApproved, string currentUsuarioId);

        // =========================================================================
        // VOLUME MANAGEMENT
        // =========================================================================

        /// <sumario>
        /// Atualiza os metadados e o indice de artigos de uma edicao (Volume).
        /// REGRA: Apenas usuarios na lista Staff podem editar.
        /// </summary>
        /// <param name="updatedVolume">O objeto Volume com os dados atualizados.</param>
        /// <param name="currentUsuarioId">O ID do usuario externo tentando atualizar.</param>
        Task<bool> AtualizarMetadadosVolumeAsync(Volume updatedVolume, string currentUsuarioId);

        // =========================================================================
        // STAFF MANAGEMENT
        // =========================================================================

        /// <sumario>
        /// Cria um novo registro de Staff para um usuário externo e define sua função de trabalho.
        /// REGRA: Apenas Administradores podem executar esta ação diretamente.
        /// </summary>
        /// <param name="usuarioId">O ID do usuário externo a ser promovido.</param>
        /// <param name="job">A função de trabalho inicial (e.g., EditorBolsista).</param>
        /// <param name="currentUsuarioId">O ID do usuário externo que está realizando a promoção (requer ser Administrador).</param>
        Task<Staff> CriarNovoStaffAsync(string usuarioId, FuncaoTrabalho job, string currentUsuarioId);
    }
}