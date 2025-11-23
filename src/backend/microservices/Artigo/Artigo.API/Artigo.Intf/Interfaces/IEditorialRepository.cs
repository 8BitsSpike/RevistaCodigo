using System.Collections.Generic;
using System.Threading.Tasks;
using Artigo.Intf.Entities;
using Artigo.Intf.Enums;

namespace Artigo.Intf.Interfaces
{
    public interface IEditorialRepository
    {
        Task<Editorial?> GetByIdAsync(string id, object? sessionHandle = null);
        Task<Editorial?> GetByArtigoIdAsync(string artigoId, object? sessionHandle = null);
        Task<IReadOnlyList<Editorial>> GetByIdsAsync(IReadOnlyList<string> ids, object? sessionHandle = null);
        Task AddAsync(Editorial editorial, object? sessionHandle = null);
        Task<bool> UpdatePositionAsync(string editorialId, PosicaoEditorial newPosition, object? sessionHandle = null);
        Task<bool> UpdateHistoryAsync(string editorialId, string newHistoryId, List<string> allHistoryIds, object? sessionHandle = null);
        Task<bool> AddCommentIdAsync(string editorialId, string commentId, object? sessionHandle = null);
        Task<bool> UpdateTeamAsync(string editorialId, EditorialTeam team, object? sessionHandle = null);
        Task<bool> DeleteAsync(string id, object? sessionHandle = null);
        Task<bool> DeleteByArtigoIdAsync(string artigoId, object? sessionHandle = null);
    }
}