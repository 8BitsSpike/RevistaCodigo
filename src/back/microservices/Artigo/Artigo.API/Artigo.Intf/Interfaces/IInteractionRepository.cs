using System.Collections.Generic;
using System.Threading.Tasks;
using Artigo.Intf.Entities;

namespace Artigo.Intf.Interfaces
{
    /// <sumario>
    /// Define o contrato para operações de persistência de dados na coleção Interaction.
    /// É responsável por gerenciar comentários públicos e comentários internos (editoriais).
    /// </sumario>
    public interface IInteractionRepository
    {
        /// <sumario>
        /// Retorna um comentario pelo seu ID.
        /// </sumario>
        Task<Interaction?> GetByIdAsync(string id, object? sessionHandle = null);

        /// <sumario>
        /// Retorna todos os comentários (públicos e editoriais) associados a um Artigo.
        /// </sumario>
        Task<IReadOnlyList<Interaction>> GetByArtigoIdAsync(string artigoId, object? sessionHandle = null);

        /// <sumario>
        /// Retorna respostas de comentários com base em uma lista de IDs de comentários "pai".
        /// </sumario>
        Task<IReadOnlyList<Interaction>> GetByParentIdsAsync(IReadOnlyList<string> parentIds, object? sessionHandle = null);

        /// <sumario>
        /// Retorna todos os comentários (públicos e editoriais) associados a multiplos Artigos.
        /// </sumario>
        Task<IReadOnlyList<Interaction>> GetByArtigoIdsAsync(IReadOnlyList<string> artigoIds, object? sessionHandle = null);

        /// <sumario>
        /// Retorna multiplos comentários com base em uma lista de IDs.
        /// </sumario>
        Task<IReadOnlyList<Interaction>> GetByIdsAsync(IReadOnlyList<string> ids, object? sessionHandle = null);

        /// <sumario>
        /// Adiciona um novo comentário (seja público ou editorial).
        /// </sumario>
        Task AddAsync(Interaction interaction, object? sessionHandle = null);

        /// <sumario>
        /// Atualiza um comentário existente.
        /// </sumario>
        Task<bool> UpdateAsync(Interaction interaction, object? sessionHandle = null);

        /// <sumario>
        /// Remove um comentário.
        /// </sumario>
        Task<bool> DeleteAsync(string id, object? sessionHandle = null);

        /// <sumario>
        /// Remove todos os comentários associados a um ArtigoId.
        /// </sumario>
        Task<bool> DeleteByArtigoIdAsync(string artigoId, object? sessionHandle = null);

        /// <sumario>
        /// Retorna uma lista paginada de comentários públicos para um artigo específico.
        /// </sumario>
        Task<IReadOnlyList<Interaction>> GetPublicCommentsAsync(string artigoId, int pagina, int tamanho, object? sessionHandle = null);
    }
}