using System;
using System.Data;
using NHibernate;
using Ninject;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    internal class PlainPostgresTransactionManager : AbstractSessionProvider, IPlainPostgresTransactionManager, IPlainSessionProvider
    {

        public PlainPostgresTransactionManager([Named(PostgresPlainStorageModule.SessionFactoryName)]ISessionFactory sessionFactory) 
            : base(sessionFactory)
        {
        }

        public void BeginTransaction()
        {
            base.CreateSession();
            this.lazySession.Value.BeginTransaction(IsolationLevel.ReadCommitted);
            this.lazySession.Value.FlushMode = FlushMode.Commit;
        }

        public void CommitTransaction()
        {
            if (!this.TransactionStarted)
                throw new InvalidOperationException("Trying to commit transaction without beginning it");

            if (this.lazySession.IsValueCreated)
            {
                this.lazySession.Value.Transaction.Commit();
                this.lazySession.Value.Close();
            }

            this.lazySession = null;
        }


        public void RollbackTransaction()
        {
            if (!this.TransactionStarted)
                throw new InvalidOperationException("Trying to rollback transaction without beginning it");

            if (this.lazySession.IsValueCreated)
            {
                this.lazySession.Value.Transaction.Rollback();
                this.lazySession.Value.Close();
            }

            this.lazySession = null;
        }
    }
}