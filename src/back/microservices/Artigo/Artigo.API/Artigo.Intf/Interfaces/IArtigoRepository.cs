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
        Task<IReadOnlyList<Artigo.Intf.Entities.Artigo>> GetByIdsAsync(IReadOnlyList<string> ids, object? sessionHandle = null);
        Task AddAsync(Artigo.Intf.Entities.Artigo artigo, object? sessionHandle = null);
        Task<bool> UpdateAsync(Artigo.Intf.Entities.Artigo artigo, object? sessionHandle = null);
        Task<bool> UpdateMetricsAsync(string id, int totalComentarios, int totalInteracoes, object? sessionHandle = null);
        Task<bool> DeleteAsync(string id, object? sessionHandle = null);
    }
}