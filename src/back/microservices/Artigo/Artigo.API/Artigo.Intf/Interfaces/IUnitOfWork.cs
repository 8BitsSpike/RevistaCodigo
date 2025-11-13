using System;
using System.Threading.Tasks;

namespace Artigo.Intf.Interfaces
{
    /// <sumario>
    /// Define um contrato para a Unidade de Trabalho (Unit of Work).
    /// Gerencia o ciclo de vida da transação de banco de dados.
    /// </sumario>
    public interface IUnitOfWork : IDisposable
    {
        /// <sumario>
        /// Verifica se uma transação está atualmente ativa e em andamento.
        /// </sumario>
        bool IsInTransaction { get; }

        /// <sumario>
        /// Inicia uma nova transação.
        /// </sumario>
        Task StartTransactionAsync();

        /// <sumario>
        /// Confirma (commita) a transação atual.
        /// </sumario>
        Task CommitTransactionAsync();

        /// <sumario>
        /// Aborta (reverte) a transação atual.
        /// </sumario>
        Task AbortTransactionAsync();

        /// <sumario>
        /// Expõe a sessão de transação subjacente para os repositórios.
        /// O tipo é 'object' para evitar uma dependência direta do MongoDB no 'Intf'.
        /// </sumario>
        object? GetSessionHandle();
    }
}