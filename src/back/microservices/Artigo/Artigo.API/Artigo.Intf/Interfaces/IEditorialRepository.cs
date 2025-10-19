using System.Collections.Generic;
using System.Threading.Tasks;
using Artigo.Intf.Entities;
using Artigo.Intf.Enums;
using System;

namespace Artigo.Intf.Interfaces
{
    /// <sumario>
    /// Define o contrato para operacoes de persistencia de dados na colecao Editorial.
    /// É responsavel por gerenciar o ciclo de revisao, a equipe editorial e o historico de versoes.
    /// </sumario>
    public interface IEditorialRepository
    {
        /// <sumario>
        /// Retorna o registro Editorial pelo seu ID local.
        /// Adicionado para suportar a busca no ArtigoService.
        /// </sumario>
        /// <param name="id">O ID do registro Editorial.</param>
        Task<Editorial?> GetByIdAsync(string id);

        /// <sumario>
        /// Retorna o registro Editorial associado a um Artigo específico.
        /// </sumario>
        /// <param name="artigoId">O ID do artigo principal.</param>
        Task<Editorial?> GetByArtigoIdAsync(string artigoId);

        /// <sumario>
        /// Retorna multiplos registros Editorial com base em uma lista de IDs.
        /// </sumario>
        /// <param name="ids">Lista de IDs editoriais.</param>
        Task<IReadOnlyList<Editorial>> GetByIdsAsync(IReadOnlyList<string> ids);

        /// <sumario>
        /// Adiciona um novo registro Editorial.
        /// Chamado durante a criação inicial de um Artigo.
        /// </sumario>
        /// <param name="editorial">O objeto Editorial a ser criado.</param>
        Task AddAsync(Editorial editorial);

        // --- Metodos de Atualizacao Granular (Substituem o UpdateAsync generico) ---

        /// <sumario>
        /// Atualiza apenas a Position e a data LastUpdated do registro Editorial.
        /// </sumario>
        // CORRIGIDO: EditorialPosition -> PosicaoEditorial
        Task<bool> UpdatePositionAsync(string editorialId, PosicaoEditorial newPosition);

        /// <sumario>
        /// Atualiza a referencia CurrentHistoryId e a lista HistoryIds.
        /// </sumario>
        Task<bool> UpdateHistoryAsync(string editorialId, string newHistoryId, List<string> allHistoryIds);

        /// <sumario>
        /// Adiciona um ID de comentário à lista CommentIds atomicamente.
        /// </summary>
        Task<bool> AddCommentIdAsync(string editorialId, string commentId);

        /// <sumario>
        /// Atualiza o objeto embutido EditorialTeam.
        /// </summary>
        Task<bool> UpdateTeamAsync(string editorialId, EditorialTeam team);

        // --- Metodos de Remocao ---

        /// <sumario>
        /// Remove um registro Editorial pelo ID do Artigo relacionado.
        /// </summary>
        Task<bool> DeleteByArtigoIdAsync(string artigoId);

        /// <sumario>
        /// Remove um registro Editorial (Raramente usado; geralmente apenas arquivado).
        /// </sumario>
        /// <param name="id">O ID do registro Editorial.</param>
        Task<bool> DeleteAsync(string id);
    }
}