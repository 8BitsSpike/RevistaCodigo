using System.Collections.Generic;
using System.Threading.Tasks;
using Artigo.Intf.Entities;
using Artigo.Intf.Enums;

namespace Artigo.Intf.Interfaces
{
    public interface IVolumeRepository
    {
        Task<Volume?> GetByIdAsync(string id, object? sessionHandle = null);
        Task<IReadOnlyList<Volume>> GetByYearAsync(int year, int pagina, int tamanho, object? sessionHandle = null);
        Task<IReadOnlyList<Volume>> GetAllAsync(int pagina, int tamanho, object? sessionHandle = null);
        Task<IReadOnlyList<Volume>> ObterVolumesListAsync(int pagina, int tamanho, object? sessionHandle = null);

        /// <sumario>
        /// Retorna volumes para o 'Card List Format' filtrados por StatusVolume.
        /// </sumario>
        Task<IReadOnlyList<Volume>> ObterVolumesPorStatusAsync(StatusVolume status, int pagina, int tamanho, object? sessionHandle = null);

        Task AddAsync(Volume newVolume, object? sessionHandle = null);
        Task<bool> UpdateAsync(Volume updatedVolume, object? sessionHandle = null);
        Task<bool> DeleteAsync(string id, object? sessionHandle = null);
        Task<IReadOnlyList<Volume>> GetByIdsAsync(IReadOnlyList<string> ids, object? sessionHandle = null);
    }
}