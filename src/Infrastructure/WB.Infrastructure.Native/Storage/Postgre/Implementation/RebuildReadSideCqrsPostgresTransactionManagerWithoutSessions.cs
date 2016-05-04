using System;
using System.Threading;
using NHibernate;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    internal class RebuildReadSideCqrsPostgresTransactionManagerWithoutSessions : ICqrsPostgresTransactionManager, IDisposable
    {
        private int startedCommandTransactions;

        public RebuildReadSideCqrsPostgresTransactionManagerWithoutSessions() {}

        public void BeginCommandTransaction()
        {
            Interlocked.Increment(ref this.startedCommandTransactions);
        }

        public void CommitCommandTransaction()
        {
            Interlocked.Decrement(ref this.startedCommandTransactions);
        }

        public void RollbackCommandTransaction()
        {
            bool isAnythingToRollback = this.startedCommandTransactions > 0;
            if (!isAnythingToRollback)
                throw new InvalidOperationException($"{this.startedCommandTransactions} command transactions are started. Nothing to rollback.");

            Interlocked.Decrement(ref this.startedCommandTransactions);
        }

        public void BeginQueryTransaction()
        {
            throw new NotSupportedException("Queries are not allowed during read side rebuild.");
        }

        public void RollbackQueryTransaction()
        {
            throw new NotSupportedException("Queries are not allowed during read side rebuild.");
        }

        public ISession GetSession()
        {
            throw new NotSupportedException("Sessions are not allowed during read side rebuild because everything is built to ESENT cache.");
        }

        public bool IsQueryTransactionStarted => false;

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        ~RebuildReadSideCqrsPostgresTransactionManagerWithoutSessions()
        {
            this.Dispose();
        }
    }
}