using System.Collections.Generic;
using System.Threading.Tasks;
using Artigo.Intf.Entities;
using Artigo.Intf.Enums;

namespace Artigo.Intf.Interfaces
{
    /// <sumario>
    /// Define o contrato para operacoes de persistencia de dados na colecao Artigo.
    /// Esta interface isola a logica de negocio (Application) dos detalhes do banco de dados (Infrastructure).
    /// </sumario>
    public interface IArtigoRepository
    {
        /// <sumario>
        /// Retorna um Artigo pelo seu ID.
        /// </sumario>
        /// <param name="id">O identificador hexadecimal do artigo.</param>
        /// <returns>O objeto Artigo ou null se nao encontrado.</returns>
        Task<Artigo?> GetByIdAsync(string id);

        /// <sumario>
        /// Retorna todos os Artigos que estao em um status especifico.
        /// </sumario>
        Task<IReadOnlyList<Artigo>> GetByStatusAsync(ArtigoStatus status);

        /// <sumario>
        /// Retorna multiplos Artigos com base em uma lista de IDs.
        /// Essencial para DataLoaders e bulk retrieval.
        /// </sumario>
        /// <param name="ids">Lista de IDs de artigos.</param>
        Task<IReadOnlyList<Artigo>> GetByIdsAsync(IReadOnlyList<string> ids);

        /// <sumario>
        /// Adiciona um novo Artigo a colecao.
        /// </sumario>
        /// <param name="artigo">O objeto Artigo a ser criado.</param>
        Task AddAsync(Artigo artigo);

        /// <sumario>
        /// Atualiza um Artigo existente (metadata e referencias).
        /// </sumario>
        /// <param name="artigo">O objeto Artigo com os dados atualizados.</param>
        Task<bool> UpdateAsync(Artigo artigo);

        /// <sumario>
        /// Atualiza apenas as métricas de interação e comentário do artigo (otimização).
        /// </sumario>
        Task<bool> UpdateMetricsAsync(string id, int totalComentarios, int totalInteracoes);

        /// <sumario>
        /// Remove um Artigo pelo seu ID.
        /// </sumario>
        /// <param name="id">O identificador hexadecimal do artigo a ser removido.</param>
        Task<bool> DeleteAsync(string id);
    }
}