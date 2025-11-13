using Artigo.Intf.Interfaces;
using MongoDB.Driver;
using MongoDB.Driver.Core.Clusters; // Adicionado para ClusterType
using System;
using System.Linq; // Adicionado para .Any()
using System.Threading.Tasks;

namespace Artigo.DbContext.Data
{
    /// <sumario>
    /// Implementação da Unidade de Trabalho (Unit of Work) para o MongoDB.
    /// Gerencia o ciclo de vida da transação (IClientSessionHandle).
    /// </sumario>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IMongoClient _mongoClient;
        private IClientSessionHandle? _sessionHandle;

        public bool IsInTransaction => _sessionHandle != null && _sessionHandle.IsInTransaction;

        public UnitOfWork(IMongoClient mongoClient)
        {
            _mongoClient = mongoClient;
        }

        /// <sumario>
        /// Inicia uma nova transação.
        /// Se o servidor (standalone) não suportar, ele continuará sem uma transação.
        /// </sumario>
        public async Task StartTransactionAsync()
        {
            if (IsInTransaction)
            {
                throw new InvalidOperationException("Uma transação já está em andamento.");
            }

            _sessionHandle = await _mongoClient.StartSessionAsync();

            try
            {
                // Tenta iniciar a transação
                _sessionHandle.StartTransaction();
            }
            catch (NotSupportedException)
            {
                // O servidor é Standalone.
                // Aborta a sessão (não a transação) e a define como nula.
                _sessionHandle.Dispose();
                _sessionHandle = null;
            }
        }

        /// <sumario>
        /// Confirma (commita) a transação atual.
        /// </sumario>
        public async Task CommitTransactionAsync()
        {
            if (!IsInTransaction)
            {
                // Se não havia transação (ex: Standalone), não há nada a fazer.
                return;
            }

            await _sessionHandle!.CommitTransactionAsync();
        }

        /// <sumario>
        /// Aborta (reverte) a transação atual.
        /// </sumario>
        public async Task AbortTransactionAsync()
        {
            if (!IsInTransaction)
            {
                // Se não havia transação (ex: Standalone), não há nada a fazer.
                return;
            }

            await _sessionHandle!.AbortTransactionAsync();
        }

        /// <sumario>
        /// Retorna o 'handle' da sessão do MongoDB, *ou nulo se não houver transação*.
        /// </sumario>
        public object? GetSessionHandle()
        {
            // Retorna a sessão apenas se ela existir e estiver em uma transação
            return IsInTransaction ? _sessionHandle : null;
        }

        /// <sumario>
        /// Libera a sessão.
        /// </sumario>
        public void Dispose()
        {
            _sessionHandle?.Dispose();
            _sessionHandle = null;
        }
    }
}