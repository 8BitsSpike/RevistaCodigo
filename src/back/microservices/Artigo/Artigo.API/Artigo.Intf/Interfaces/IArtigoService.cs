using System.Collections.Generic;
using System.Threading.Tasks;
using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using Artigo.Intf.Inputs;
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

        Task<Artigo.Intf.Entities.Artigo> CreateArtigoAsync(Artigo.Intf.Entities.Artigo artigo, string conteudoInicial, List<MidiaEntry> midiasCompletas, List<Autor> autores, string currentUsuarioId, string commentary);

        Task<bool> AtualizarMetadadosArtigoAsync(string artigoId, UpdateArtigoMetadataInput input, string currentUsuarioId, string commentary);

        Task<bool> AtualizarConteudoArtigoAsync(string artigoId, string newContent, List<MidiaEntry> midias, string currentUsuarioId, string commentary);

        Task<bool> AlterarStatusArtigoAsync(string artigoId, StatusArtigo newStatus, string currentUsuarioId, string commentary);

        Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> ObterArtigosPorStatusAsync(StatusArtigo status, int pagina, int tamanho, string currentUsuarioId);

        Task<Editorial> AtualizarEquipeEditorialAsync(string artigoId, EditorialTeam team, string currentUsuarioId, string commentary);


        // =========================================================================
        // *** NOVAS QUERIES DE "FORMATO" (DTOs de Leitura Pública) ***
        // =========================================================================

        Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> ObterArtigosCardListAsync(int pagina, int tamanho);

        Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> ObterArtigosCardListPorTipoAsync(TipoArtigo tipo, int pagina, int tamanho);

        Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> ObterArtigosCardListPorTituloAsync(string searchTerm, int pagina, int tamanho);

        Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> ObterArtigosCardListPorNomeAutorAsync(string searchTerm, int pagina, int tamanho);

        Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> ObterArtigosPorListaIdsAsync(IReadOnlyList<string> ids);

        /// <sumario>
        /// Busca artigos (card) de um autor específico usando seu UsuarioId.
        /// Retorna todos os status (rascunho, publicado, etc.)
        /// </sumario>
        Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> ObterMeusArtigosCardListAsync(string currentUsuarioId);


        Task<Autor?> ObterAutorCardAsync(string autorId);

        Task<Volume?> ObterVolumeCardAsync(string volumeId);

        Task<Artigo.Intf.Entities.Artigo?> ObterArtigoViewAsync(string artigoId);

        Task<Volume?> ObterVolumeViewAsync(string volumeId);

        Task<Artigo.Intf.Entities.Artigo?> ObterArtigoEditorialViewAsync(string artigoId, string currentUsuarioId);

        Task<IReadOnlyList<Volume>> ObterVolumesListAsync(int pagina, int tamanho);

        // --- (NOVOS MÉTODOS PARA STAFF) ---

        /// <sumario>
        /// (STAFF) Retorna artigos (card) filtrados por TipoArtigo, SEM filtro de status.
        /// </sumario>
        Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> ObterArtigosEditorialPorTipoAsync(TipoArtigo tipo, int pagina, int tamanho, string currentUsuarioId);

        /// <sumario>
        /// (STAFF) Busca artigos (card) por Título, SEM filtro de status.
        /// </sumario>
        Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> SearchArtigosEditorialByTitleAsync(string searchTerm, int pagina, int tamanho, string currentUsuarioId);

        /// <sumario>
        /// (STAFF) Busca artigos (card) por IDs de Autor, SEM filtro de status.
        /// </sumario>
        Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> SearchArtigosEditorialByAutorIdsAsync(IReadOnlyList<string> autorIds, int pagina, int tamanho, string currentUsuarioId);

        // =========================================================================
        // INTERACTION (COMENTARIOS) MANAGEMENT
        // =========================================================================

        Task<Interaction> CriarComentarioPublicoAsync(string artigoId, Interaction newComment, string? parentCommentId);
        Task<Interaction> CriarComentarioEditorialAsync(string artigoId, Interaction newComment, string currentUsuarioId);

        Task<Interaction> AtualizarInteracaoAsync(string interacaoId, string newContent, string currentUsuarioId, string commentary);

        Task<bool> DeletarInteracaoAsync(string interacaoId, string currentUsuarioId, string commentary);

        Task<IReadOnlyList<Interaction>> ObterComentariosPublicosAsync(string artigoId, int pagina, int tamanho);


        // =========================================================================
        // MÉTODOS (StaffComentario)
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

        Task<bool> AtualizarMetadadosVolumeAsync(string volumeId, UpdateVolumeMetadataInput input, string currentUsuarioId, string commentary);

        Task<Volume> CriarVolumeAsync(Volume novoVolume, string currentUsuarioId, string commentary);
        Task<IReadOnlyList<Volume>> ObterVolumesAsync(int pagina, int tamanho, string currentUsuarioId);
        Task<IReadOnlyList<Volume>> ObterVolumesPorAnoAsync(int ano, int pagina, int tamanho, string currentUsuarioId);
        Task<Volume?> ObterVolumePorIdAsync(string idVolume, string currentUsuarioId);
        Task<IReadOnlyList<Volume>> ObterVolumesPorStatusAsync(StatusVolume status, int pagina, int tamanho, string currentUsuarioId);


        // =========================================================================
        // STAFF MANAGEMENT
        // =========================================================================

        Task<Staff> CriarNovoStaffAsync(Staff novoStaff, string currentUsuarioId, string commentary);

        /// <sumario>
        /// Atualiza o registro de um membro da Staff (Job ou IsActive).
        /// </sumario>
        Task<Staff> AtualizarStaffAsync(UpdateStaffInput input, string currentUsuarioId, string commentary);

        Task<IReadOnlyList<Autor>> ObterAutoresAsync(int pagina, int tamanho, string currentUsuarioId);
        Task<Autor?> ObterAutorPorIdAsync(string idAutor, string currentUsuarioId);

        Task<Staff?> ObterStaffPorIdAsync(string staffId, string currentUsuarioId);

        Task<IReadOnlyList<Staff>> ObterStaffListAsync(int pagina, int tamanho, string currentUsuarioId);

        /// <sumario>
        /// Verifica se o usuário autenticado é um membro ativo da equipe Staff.
        /// </sumario>
        Task<bool> VerificarStaffAsync(string currentUsuarioId);
    }
}