using System.Collections.Generic;
using System.Threading.Tasks;
using Artigo.Intf.Entities;
using Artigo.Intf.Enums;

namespace Artigo.Intf.Interfaces
{
    /// <sumario>
    /// Define o contrato para operacoes de persistencia de dados na colecao Pending.
    /// É responsavel por gerenciar as requisições de acoes criticas que necessitam de aprovacao staff.
    /// </sumario>
    public interface IPendingRepository
    {
        /// <sumario>
        /// Retorna uma requisição pendente pelo seu ID.
        /// Usado para verificar a existencia e o status antes de uma acao de aprovacao/rejeicao.
        /// </sumario>
        /// <param name="id">O identificador hexadecimal da requisição pendente.</param>
        Task<Pending?> GetByIdAsync(string id);

        /// <sumario>
        /// Retorna uma lista de requisições pendentes, tipicamente filtradas por status (e.g., AguardandoRevisao).
        /// </sumario>
        // CORRIGIDO: PendingStatus -> StatusPendente
        /// <param name="status">Filtra pelo status da requisição.</param>
        Task<IReadOnlyList<Pending>> GetByStatusAsync(StatusPendente status);

        /// <sumario>
        /// Adiciona uma nova requisição pendente.
        /// Chamado principalmente por EditorBolsistas.
        /// </sumario>
        /// <param name="pendingItem">O objeto Pending a ser criado.</param>
        Task AddAsync(Pending pendingItem);

        /// <sumario>
        /// Atualiza o status (aprovacao/rejeicao/arquivamento) e possivelmente o comando de uma requisição.
        /// Usado por EditorChefes e Administradores.
        /// </sumario>
        /// <param name="pendingItem">O objeto Pending com os dados atualizados.</param>
        Task<bool> UpdateAsync(Pending pendingItem);

        /// <sumario>
        /// Remove uma requisição pendente.
        /// Usado para limpeza ou se uma requisição for arquivada/cancelada.
        /// </sumario>
        /// <param name="id">O ID da requisição a ser removida.</param>
        Task<bool> DeleteAsync(string id);
    }
}