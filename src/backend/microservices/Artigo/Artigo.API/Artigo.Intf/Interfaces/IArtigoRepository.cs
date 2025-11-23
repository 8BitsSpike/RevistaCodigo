using System.Collections.Generic;
using System.Threading.Tasks;
using Artigo.Intf.Entities;
using Artigo.Intf.Enums;

namespace Artigo.Intf.Interfaces
{
    public interface IArtigoRepository
    {
        Task<Artigo.Intf.Entities.Artigo?> GetByIdAsync(string id, object? sessionHandle = null);
        Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> GetByStatusAsync(StatusArtigo status, int pagina, int tamanho, object? sessionHandle = null);
        Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> ObterArtigosCardListAsync(int pagina, int tamanho, object? sessionHandle = null);

        /// <sumario>
        /// (PÚBLICO) Retorna artigos (card) filtrados por TipoArtigo e Status PUBLICADO.
        /// </sumario>
        Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> ObterArtigosCardListPorTipoAsync(TipoArtigo tipo, int pagina, int tamanho, object? sessionHandle = null);

        Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> GetByIdsAsync(IReadOnlyList<string> ids, object? sessionHandle = null);
        Task AddAsync(Artigo.Intf.Entities.Artigo artigo, object? sessionHandle = null);
        Task<bool> UpdateAsync(Artigo.Intf.Entities.Artigo artigo, object? sessionHandle = null);
        Task<bool> UpdateMetricsAsync(string id, int totalComentarios, int totalInteracoes, object? sessionHandle = null);
        Task<bool> DeleteAsync(string id, object? sessionHandle = null);

        /// <sumario>
        /// (PÚBLICO) Busca artigos (card) por Título, filtrado por Status PUBLICADO.
        /// </sumario>
        Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> SearchArtigosCardListByTitleAsync(string searchTerm, int pagina, int tamanho, object? sessionHandle = null);

        /// <sumario>
        /// (PÚBLICO) Busca artigos (card) por IDs de Autor, filtrado por Status PUBLICADO.
        /// </sumario>
        Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> SearchArtigosCardListByAutorIdsAsync(IReadOnlyList<string> autorIds, object? sessionHandle = null);

        /// <sumario>
        /// (STAFF) Busca artigos (card) de um único Autor. NÃO FILTRA POR STATUS.
        /// </sumario>
        Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> ObterArtigosCardListPorAutorIdAsync(string autorId, object? sessionHandle = null);

        /// <sumario>
        /// (PÚBLICO) Busca artigos (card) por Referência de Autor, filtrado por Status PUBLICADO.
        /// </sumario>
        Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> SearchArtigosCardListByAutorReferenceAsync(string searchTerm, object? sessionHandle = null);

        // --- (NOVOS MÉTODOS PARA STAFF) ---

        /// <sumario>
        /// (STAFF) Retorna artigos (card) filtrados por TipoArtigo, SEM filtro de status.
        /// </sumario>
        Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> ObterArtigosEditorialPorTipoAsync(TipoArtigo tipo, int pagina, int tamanho, object? sessionHandle = null);

        /// <sumario>
        /// (STAFF) Busca artigos (card) por Título, SEM filtro de status.
        /// </sumario>
        Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> SearchArtigosEditorialByTitleAsync(string searchTerm, int pagina, int tamanho, object? sessionHandle = null);

        /// <sumario>
        /// (STAFF) Busca artigos (card) por IDs de Autor, SEM filtro de status.
        /// </sumario>
        Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> SearchArtigosEditorialByAutorIdsAsync(IReadOnlyList<string> autorIds, int pagina, int tamanho, object? sessionHandle = null);
    }
}