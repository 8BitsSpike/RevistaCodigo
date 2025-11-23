using System.Collections.Generic;
using System.Threading.Tasks;
using Artigo.Intf.Entities;

namespace Artigo.Intf.Interfaces
{
    public interface IAutorRepository
    {
        Task<Autor?> GetByIdAsync(string id, object? sessionHandle = null);
        Task<IReadOnlyList<Autor>> GetByIdsAsync(IReadOnlyList<string> ids, object? sessionHandle = null);
        Task<Autor?> GetByUsuarioIdAsync(string usuarioId, object? sessionHandle = null);
        Task<IReadOnlyList<Autor>> GetAllAsync(int pagina, int tamanho, object? sessionHandle = null);
        Task<Autor> UpsertAsync(Autor autor, object? sessionHandle = null);
        Task<bool> DeleteAsync(string id, object? sessionHandle = null);

        /// <sumario>
        /// Busca autores (registrados) pelo nome, usando regex.
        /// </sumario>
        Task<IReadOnlyList<Autor>> SearchAutoresByNameAsync(string searchTerm, object? sessionHandle = null);
    }
}