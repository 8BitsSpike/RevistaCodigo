using System.Collections.Generic;
using System.Threading.Tasks;
using Artigo.Intf.Entities;
using Artigo.Intf.Enums;

namespace Artigo.Intf.Interfaces
{
    public interface IPendingRepository
    {
        Task<Pending?> GetByIdAsync(string id, object? sessionHandle = null);
        Task<IReadOnlyList<Pending>> GetAllAsync(int pagina, int tamanho, object? sessionHandle = null);
        Task<IReadOnlyList<Pending>> BuscarPendenciasPorStatus(StatusPendente status, int pagina, int tamanho, object? sessionHandle = null);
        Task<IReadOnlyList<Pending>> BuscarPendenciaPorEntidadeId(string targetEntityId, object? sessionHandle = null);
        Task<IReadOnlyList<Pending>> BuscarPendenciaPorTipoDeEntidade(TipoEntidadeAlvo targetType, object? sessionHandle = null);
        Task<IReadOnlyList<Pending>> BuscarPendenciaPorRequisitanteId(string requesterUsuarioId, object? sessionHandle = null);
        Task AddAsync(Pending pendingRequest, object? sessionHandle = null);
        Task<bool> UpdateAsync(Pending pendingRequest, object? sessionHandle = null);
        Task<bool> DeleteAsync(string id, object? sessionHandle = null);
    }
}