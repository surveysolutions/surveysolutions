using System;
using System.Data;
using NHibernate;
using Ninject;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    internal class CqrsPostgresTransactionManager : AbstractSessionProvider, ICqrsPostgresTransactionManager
    {
        private bool triedToBeginCommandTransaction;

        public CqrsPostgresTransactionManager([Named(PostgresReadSideModule.ReadSideSessionFactoryName)]ISessionFactory sessionFactory) 
            : base(sessionFactory)
        {
        }

        public void BeginCommandTransaction()
        {
            this.triedToBeginCommandTransaction = true;

            base.CreateSession();
        }

        public void CommitCommandTransaction()
        {
            if (this.lazySession == null)
                throw new InvalidOperationException();

            if (this.lazySession.IsValueCreated)
            {
                this.lazySession.Value.Transaction.Commit();
                this.lazySession.Value.Close();
            }

            this.lazySession = null;

            this.triedToBeginCommandTransaction = false;
        }

        public void RollbackCommandTransaction()
        {
            if (!this.triedToBeginCommandTransaction)
                throw new InvalidOperationException("Command transaction is not started and therefore cannot be rolled back.");

            if (this.lazySession?.IsValueCreated == true)
            {
                this.lazySession.Value.Transaction.Rollback();
                this.lazySession.Value.Close();
            }

            this.lazySession = null;

            this.triedToBeginCommandTransaction = false;
        }

        ~CqrsPostgresTransactionManager()
        {
            this.Dispose();
        }

        protected override void InitializeSessionSettings(ISession session)
        {
            session.BeginTransaction(IsolationLevel.RepeatableRead);
            session.FlushMode = FlushMode.Commit;
        }
    }
}