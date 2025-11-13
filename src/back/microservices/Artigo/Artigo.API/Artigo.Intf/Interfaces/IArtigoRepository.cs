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
        /// Retorna artigos para o 'Card List Format' filtrados por TipoArtigo.
        /// </sumario>
        Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> ObterArtigosCardListPorTipoAsync(TipoArtigo tipo, int pagina, int tamanho, object? sessionHandle = null);

        Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> GetByIdsAsync(IReadOnlyList<string> ids, object? sessionHandle = null);
        Task AddAsync(Artigo.Intf.Entities.Artigo artigo, object? sessionHandle = null);
        Task<bool> UpdateAsync(Artigo.Intf.Entities.Artigo artigo, object? sessionHandle = null);
        Task<bool> UpdateMetricsAsync(string id, int totalComentarios, int totalInteracoes, object? sessionHandle = null);
        Task<bool> DeleteAsync(string id, object? sessionHandle = null);

        Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> SearchArtigosCardListByTitleAsync(string searchTerm, int pagina, int tamanho, object? sessionHandle = null);

        /// <sumario>
        /// Busca artigos (formato card) que correspondem a uma lista de IDs de Autores (registrados).
        /// FILTRA APENAS StatusArtigo.Publicado.
        /// </sumario>
        Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> SearchArtigosCardListByAutorIdsAsync(IReadOnlyList<string> autorIds, object? sessionHandle = null);

        /// <sumario>
        /// (NOVO) Busca artigos (formato card) de um único Autor.
        /// NÃO FILTRA POR STATUS - Retorna todos (Rascunho, Publicado, etc.)
        /// </sumario>
        Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> ObterArtigosCardListPorAutorIdAsync(string autorId, object? sessionHandle = null);


        /// <sumario>
        /// Busca artigos (formato card) que correspondem a um termo de busca no campo AutorReference (não registrados).
        /// </sumario>
        Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> SearchArtigosCardListByAutorReferenceAsync(string searchTerm, object? sessionHandle = null);
    }
}