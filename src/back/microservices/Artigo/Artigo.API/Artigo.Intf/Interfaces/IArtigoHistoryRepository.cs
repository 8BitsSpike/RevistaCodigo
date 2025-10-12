using System.Collections.Generic;
using System.Threading.Tasks;
using Artigo.Intf.Entities;
using Artigo.Intf.Enums;

namespace Artigo.Intf.Interfaces
{
    /// <sumario>
    /// Define o contrato para operacoes de persistencia de dados na colecao ArtigoHistory.
    /// É responsavel por gerenciar as versoes completas (corpo/conteudo) do artigo.
    /// </sumario>
    public interface IArtigoHistoryRepository
    {
        /// <sumario>
        /// Retorna o registro de historico pelo seu ID.
        /// Usado para carregar o conteudo completo de uma versao especifica.
        /// </sumario>
        /// <param name="id">O identificador hexadecimal do registro de historico.</param>
        Task<ArtigoHistory?> GetByIdAsync(string id);

        /// <sumario>
        /// Retorna uma versao especifica de um artigo com base no ArtigoId e no Enum de Versao.
        /// </sumario>
        /// <param name="artigoId">O ID do artigo principal.</param>
        /// <param name="version">O enum ArtigoVersion para a versao desejada.</param>
        Task<ArtigoHistory?> GetByArtigoAndVersionAsync(string artigoId, ArtigoVersion version);

        /// <sumario>
        /// Retorna multiplos registros de ArtigoHistory com base em uma lista de IDs.
        /// Crucial para DataLoaders que buscam todas as versoes associadas a um Editorial.
        /// </sumario>
        /// <param name="ids">Lista de IDs de historico.</param>
        Task<IReadOnlyList<ArtigoHistory>> GetByIdsAsync(IReadOnlyList<string> ids);

        /// <sumario>
        /// Adiciona um novo registro de ArtigoHistory.
        /// Chamado sempre que uma nova versao (e.g., PrimeiraEdicao) é criada.
        /// </sumario>
        /// <param name="historyEntry">O objeto ArtigoHistory a ser criado.</param>
        Task AddAsync(ArtigoHistory historyEntry);

        /// <sumario>
        /// Atualiza o conteudo de um registro de ArtigoHistory existente.
        /// Raramente usado, mas necessario em caso de correcoes estritas antes de uma nova versao.
        /// </sumario>
        /// <param name="historyEntry">O objeto ArtigoHistory com os dados atualizados.</param>
        Task<bool> UpdateAsync(ArtigoHistory historyEntry);
    }
}
