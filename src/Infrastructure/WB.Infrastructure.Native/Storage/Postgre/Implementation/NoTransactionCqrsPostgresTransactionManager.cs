using System;
using NHibernate;
using Ninject;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    internal class NoTransactionCqrsPostgresTransactionManager : AbstractSessionProvider, ICqrsPostgresTransactionManager
    {
        private bool triedToBeginCommandTransaction;

        public NoTransactionCqrsPostgresTransactionManager([Named(PostgresReadSideModule.ReadSideSessionFactoryName)]ISessionFactory sessionFactory) 
            : base(sessionFactory)
        {
        }

        public void BeginCommandTransaction()
        {
            this.triedToBeginCommandTransaction = true;

            base.CreateSession();
            this.lazySession.Value.FlushMode = FlushMode.Never;
        }

        public void CommitCommandTransaction()
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

        public void RollbackCommandTransaction()
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

        ~NoTransactionCqrsPostgresTransactionManager()
        {
            this.Dispose();
        }
    }
}