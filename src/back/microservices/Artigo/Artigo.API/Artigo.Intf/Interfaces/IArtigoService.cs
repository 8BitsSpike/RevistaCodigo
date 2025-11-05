using System.Collections.Generic;
using System.Threading.Tasks;
using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using Artigo.Intf.Inputs; // *** ADICIONADO ***
using System;

namespace Artigo.Intf.Interfaces
{
    /// <sumario>
    /// Define o contrato para a logica de negócio da entidade Artigo.
    /// É responsável por orquestrar repositórios e aplicar regras de negócio (e.g., validação, autorização).
    /// </sumario>
    public interface IArtigoService
    {
        // =========================================================================
        // ARTIGO CORE MANAGEMENT (CRUD & STATUS)
        // =========================================================================

        Task<Artigo.Intf.Entities.Artigo?> ObterArtigoPublicadoAsync(string id);
        Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> ObterArtigosPublicadosParaVisitantesAsync(int pagina, int tamanho);
        Task<Artigo.Intf.Entities.Artigo?> ObterArtigoParaEditorialAsync(string id, string currentUsuarioId);

        Task<Artigo.Intf.Entities.Artigo> CreateArtigoAsync(Artigo.Intf.Entities.Artigo artigo, string conteudoInicial, List<Autor> autores, string currentUsuarioId, string commentary);

        /// <sumario>
        /// (ATUALIZADO) Atualiza metadados simples (titulo, resumo) de um artigo.
        /// Implementa a lógica de Pending Request.
        /// *** ASSINATURA ALTERADA para usar o Input DTO ***
        /// </sumario>
        /// <param name="artigoId">ID do artigo a ser atualizado.</param>
        /// <param name="input">O DTO de input com os campos parciais para atualização.</param>
        /// <param name="currentUsuarioId">O ID do usuário externo tentando atualizar.</param>
        /// <param name="commentary">Comentário para a mutação (usado se virar Pending).</param>
        Task<bool> AtualizarMetadadosArtigoAsync(string artigoId, UpdateArtigoMetadataInput input, string currentUsuarioId, string commentary);

        /// <sumario>
        /// (ATUALIZADO) Atualiza o corpo do artigo, criando um novo registro em ArtigoHistory.
        /// Implementa a lógica de Pending Request.
        /// </summary>
        Task<bool> AtualizarConteudoArtigoAsync(string artigoId, string newContent, string currentUsuarioId, string commentary);

        /// <sumario>
        /// (ATUALIZADO) Altera o Status (Rascunho, EmRevisao, Publicado, etc.) do artigo.
        /// Implementa a lógica de Pending Request.
        /// </summary>
        Task<bool> AlterarStatusArtigoAsync(string artigoId, StatusArtigo newStatus, string currentUsuarioId, string commentary);

        Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> ObterArtigosPorStatusAsync(StatusArtigo status, int pagina, int tamanho, string currentUsuarioId);


        // =========================================================================
        // *** NOVAS QUERIES DE "FORMATO" (DTOs de Leitura Pública) ***
        // =========================================================================

        /// <sumario>
        /// (NOVO) Retorna artigos para o 'Card List Format'. Acessível a todos.
        /// O repositório usará projeção.
        /// </sumario>
        Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> ObterArtigosCardListAsync(int pagina, int tamanho);

        /// <sumario>
        /// (NOVO) Retorna um autor para o 'Autor Card Format'.
        /// O serviço resolverá los Títulos dos ArtigoWorkIds.
        /// </sumario>
        Task<Autor?> ObterAutorCardAsync(string autorId);

        /// <sumario>
        /// (NOVO) Retorna um volume para o 'Volume Card Format'. Acessível a todos.
        /// </sumario>
        Task<Volume?> ObterVolumeCardAsync(string volumeId);

        /// <sumario>
        /// (NOVO) Retorna um artigo para o 'Artigo View Format'. Acessível a todos (se publicado).
        /// A camada de API fará a agregação.
        /// </sumario>
        Task<Artigo.Intf.Entities.Artigo?> ObterArtigoViewAsync(string artigoId);

        /// <sumario>
        /// (NOVO) Retorna um artigo para o 'Artigo Editorial View Format'.
        /// Requer permissão de Autor ou Staff.
        /// </sumario>
        Task<Artigo.Intf.Entities.Artigo?> ObterArtigoEditorialViewAsync(string artigoId, string currentUsuarioId);

        /// <sumario>
        /// *** ADICIONADO (CORREÇÃO) ***
        /// (NOVO) Retorna volumes para o 'Volume List Format'. Acessível a todos.
        /// </sumario>
        Task<IReadOnlyList<Volume>> ObterVolumesListAsync(int pagina, int tamanho);


        // =========================================================================
        // INTERACTION (COMENTARIOS) MANAGEMENT
        // =========================================================================

        Task<Interaction> CriarComentarioPublicoAsync(string artigoId, Interaction newComment, string? parentCommentId);
        Task<Interaction> CriarComentarioEditorialAsync(string artigoId, Interaction newComment, string currentUsuarioId);

        /// <sumario>
        /// (NOVO) Atualiza uma interação (Comentário Público ou Editorial).
        /// REGRA: Apenas o autor pode editar.
        /// </sumario>
        Task<Interaction> AtualizarInteracaoAsync(string interacaoId, string newContent, string currentUsuarioId, string commentary);

        /// <sumario>
        /// (NOVO) Deleta uma interação (Comentário Público ou Editorial).
        /// REGRA: Autor, EditorChefe ou Admin podem deletar.
        /// REGRA: EditorBolsista cria um Pending.
        /// </sumario>
        Task<bool> DeletarInteracaoAsync(string interacaoId, string currentUsuarioId, string commentary);


        // =========================================================================
        // *** NOVOS MÉTODOS (StaffComentario) ***
        // =========================================================================

        Task<ArtigoHistory> AddStaffComentarioAsync(string historyId, string usuarioId, string comment, string? parent);
        Task<ArtigoHistory> UpdateStaffComentarioAsync(string historyId, string comentarioId, string newContent, string currentUsuarioId);
        Task<ArtigoHistory> DeleteStaffComentarioAsync(string historyId, string comentarioId, string currentUsuarioId);


        // =========================================================================
        // PENDING (FLUXO DE APROVAÇÃO) MANAGEMENT
        // =========================================================================

        Task<Pending> CriarRequisicaoPendenteAsync(Pending newRequest, string currentUsuarioId);
        Task<bool> ResolverRequisicaoPendenteAsync(string pendingId, bool isApproved, string currentUsuarioId);
        Task<IReadOnlyList<Pending>> ObterPendentesAsync(int pagina, int tamanho, string currentUsuarioId);
        Task<IReadOnlyList<Pending>> ObterPendentesPorStatusAsync(StatusPendente status, int pagina, int tamanho, string currentUsuarioId);
        Task<IReadOnlyList<Pending>> ObterPendenciasPorEntidadeIdAsync(string targetEntityId, string currentUsuarioId);
        Task<IReadOnlyList<Pending>> ObterPendenciasPorTipoDeEntidadeAsync(TipoEntidadeAlvo targetType, string currentUsuarioId);
        Task<IReadOnlyList<Pending>> ObterPendenciasPorRequisitanteIdAsync(string requesterUsuarioId, string currentUsuarioId);


        // =========================================================================
        // VOLUME MANAGEMENT
        // =========================================================================

        Task<bool> AtualizarMetadadosVolumeAsync(Volume updatedVolume, string currentUsuarioId, string commentary);
        Task<Volume> CriarVolumeAsync(Volume novoVolume, string currentUsuarioId, string commentary);
        Task<IReadOnlyList<Volume>> ObterVolumesAsync(int pagina, int tamanho, string currentUsuarioId);
        Task<IReadOnlyList<Volume>> ObterVolumesPorAnoAsync(int ano, int pagina, int tamanho, string currentUsuarioId);
        Task<Volume?> ObterVolumePorIdAsync(string idVolume, string currentUsuarioId);

        // =========================================================================
        // STAFF MANAGEMENT
        // =========================================================================

        Task<Staff> CriarNovoStaffAsync(Staff novoStaff, string currentUsuarioId, string commentary);
        Task<IReadOnlyList<Autor>> ObterAutoresAsync(int pagina, int tamanho, string currentUsuarioId);
        Task<Autor?> ObterAutorPorIdAsync(string idAutor, string currentUsuarioId);

        /// <sumario>
        /// (NOVO) Retorna um membro da Staff pelo seu ID local.
        /// REGRA: Apenas Staff pode ver.
        /// </sumario>
        Task<Staff?> ObterStaffPorIdAsync(string staffId, string currentUsuarioId);

        /// <sumario>
        /// (NOVO) Retorna uma lista paginada de todos os membros da Staff.
        /// REGRA: Apenas Staff pode ver.
        /// </sumario>
        Task<IReadOnlyList<Staff>> ObterStaffListAsync(int pagina, int tamanho, string currentUsuarioId);
    }
}