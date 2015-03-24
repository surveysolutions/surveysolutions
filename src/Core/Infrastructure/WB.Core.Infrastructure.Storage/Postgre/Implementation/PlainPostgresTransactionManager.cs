using System;
using System.Data;
using NHibernate;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.Infrastructure.Storage.Postgre.Implementation
{
    public class PlainPostgresTransactionManager : IPlainTransactionManager, IDisposable
    {
        private readonly ISession session;
        private ITransaction transaction;

        public PlainPostgresTransactionManager(ISession session)
        {
            if (session == null) throw new ArgumentNullException("session");
            this.session = session;
        }

        public void BeginTransaction()
        {
            this.transaction = this.session.BeginTransaction(IsolationLevel.ReadCommitted);
        }

        public void CommitTransaction()
        {
            if (this.transaction == null)
            {
                throw new InvalidOperationException("Trying to commit transaction without beginning it");
            }

            this.transaction.Commit();
        }

        public void RollbackTransaction()
        {
            if (this.transaction == null)
            {
                throw new InvalidOperationException("Trying to rollback transaction without beginning it");
            }

            this.transaction.Rollback();
        }

        public void Dispose()
        {
            this.session.Dispose();
        }
    }
}