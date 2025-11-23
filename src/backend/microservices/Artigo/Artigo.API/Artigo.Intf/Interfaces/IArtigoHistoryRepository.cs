using System.Collections.Generic;
using System.Threading.Tasks;
using Artigo.Intf.Entities;
using Artigo.Intf.Enums;


namespace Artigo.Intf.Interfaces
{
    /// <sumario>
    /// Define o contrato para operações de persistência de dados na coleção ArtigoHistory.
    /// É responsável por gerenciar as versões completas (corpo/conteúdo) do artigo.
    /// </sumario>
    public interface IArtigoHistoryRepository
    {
        /// <sumario>
        /// Retorna o registro de histórico pelo seu ID.
        /// </sumario>
        Task<ArtigoHistory?> GetByIdAsync(string id, object? sessionHandle = null);

        /// <sumario>
        /// Retorna um dicionário de strings de 'Content' com base em uma lista de IDs.
        /// </sumario>
        Task<IReadOnlyDictionary<string, string>> GetContentsByIdsAsync(IReadOnlyList<string> ids, object? sessionHandle = null);

        /// <sumario>
        /// Retorna uma versão específica de um artigo com base no ArtigoId e no Enum de Versão.
        /// </sumario>
        Task<ArtigoHistory?> GetByArtigoAndVersionAsync(string artigoId, VersaoArtigo version, object? sessionHandle = null);

        /// <sumario>
        /// Retorna multiplos registros de ArtigoHistory com base em uma lista de IDs.
        /// </sumario>
        Task<IReadOnlyList<ArtigoHistory>> GetByIdsAsync(IReadOnlyList<string> ids, object? sessionHandle = null);

        /// <sumario>
        /// Adiciona um novo registro de ArtigoHistory.
        /// </sumario>
        Task AddAsync(ArtigoHistory historyEntry, object? sessionHandle = null);

        /// <sumario>
        /// Atualiza o conteúdo de um registro de ArtigoHistory existente.
        /// </sumario>
        Task<bool> UpdateAsync(ArtigoHistory historyEntry, object? sessionHandle = null);

        /// <sumario>
        /// Remove todos os registros de histórico associados a um ArtigoId.
        /// </sumario>
        Task<bool> DeleteByArtigoIdAsync(string artigoId, object? sessionHandle = null);
    }
}