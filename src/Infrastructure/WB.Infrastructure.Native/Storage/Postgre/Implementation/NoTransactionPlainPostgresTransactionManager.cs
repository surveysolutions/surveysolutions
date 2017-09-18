using System;
using NHibernate;
using Ninject;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    internal class NoTransactionPlainPostgresTransactionManager : AbstractSessionProvider, IPlainPostgresTransactionManager
    {
        private bool triedToBeginCommandTransaction;

        public NoTransactionPlainPostgresTransactionManager([Named(PostgresPlainStorageModule.SessionFactoryName)]ISessionFactory sessionFactory) 
            : base(sessionFactory)
        {
        }

        public void BeginTransaction()
        {
            this.triedToBeginCommandTransaction = true;

            base.CreateSession();
        }

        public void CommitTransaction()
        {
            if (this.lazySession == null)
                throw new InvalidOperationException();

            if (this.lazySession.IsValueCreated)
            {
                this.lazySession.Value.Close();
            }

            this.lazySession = null;

            this.triedToBeginCommandTransaction = false;
        }

        public void RollbackTransaction()
        {
            if (!this.triedToBeginCommandTransaction)
                throw new InvalidOperationException();

            if (this.lazySession != null)
            {
                if (this.lazySession.IsValueCreated)
                {
                    this.lazySession.Value.Close();
                }

                this.lazySession = null;
            }

            this.triedToBeginCommandTransaction = false;
        }

        ~NoTransactionPlainPostgresTransactionManager()
        {
            this.Dispose();
        }

        protected override void InitializeSessionSettings(ISession session)
            => session.FlushMode = FlushMode.Never;
    }
}