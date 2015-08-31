using System;
using NHibernate;
using Ninject;

namespace WB.Core.Infrastructure.Storage.Postgre.Implementation
{
    internal class NoTransactionCqrsPostgresTransactionManager : ICqrsPostgresTransactionManager, IDisposable
    {
        private readonly ISessionFactory sessionFactory;

        private Lazy<ISession> lazyCommandSession;
        private Lazy<ISession> lazyQuerySession;

        private bool triedToBeginCommandTransaction;
        private bool triedToBeginQueryTransaction;

        public NoTransactionCqrsPostgresTransactionManager([Named(PostgresReadSideModule.ReadSideSessionFactoryName)]ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        public void BeginCommandTransaction()
        {
            this.triedToBeginCommandTransaction = true;

            if (this.lazyCommandSession != null)
                throw new InvalidOperationException();

            this.lazyCommandSession = new Lazy<ISession>(() => this.sessionFactory.OpenSession(), true);
        }

        public void CommitCommandTransaction()
        {
            if (this.lazyCommandSession == null)
                throw new InvalidOperationException();

            if (this.lazyCommandSession.IsValueCreated)
            {
                this.lazyCommandSession.Value.Flush();
                this.lazyCommandSession.Value.Close();
            }

            this.lazyCommandSession = null;

            this.triedToBeginCommandTransaction = false;
        }

        public void RollbackCommandTransaction()
        {
            if (!this.triedToBeginCommandTransaction)
                throw new InvalidOperationException();

            if (this.lazyCommandSession != null)
            {
                if (this.lazyCommandSession.IsValueCreated)
                {
                    this.lazyCommandSession.Value.Close();
                }

                this.lazyCommandSession = null;
            }

            this.triedToBeginCommandTransaction = false;
        }

        public void BeginQueryTransaction()
        {
            this.triedToBeginQueryTransaction = true;

            if (this.lazyQuerySession != null)
                throw new InvalidOperationException();

            this.lazyQuerySession = new Lazy<ISession>(() => this.sessionFactory.OpenSession(), true);

        }

        public void RollbackQueryTransaction()
        {
            if (!this.triedToBeginQueryTransaction)
                throw new InvalidOperationException("Query transaction is not started and therefore cannot be rolled back.");

            if (this.lazyQuerySession != null)
            {
                if (this.lazyQuerySession.IsValueCreated)
                {
                    this.lazyQuerySession.Value.Close();
                }

                this.lazyQuerySession = null;
            }

            this.triedToBeginQueryTransaction = false;
        }

        public ISession GetSession()
        {
            var session = this.lazyCommandSession ?? this.lazyQuerySession;
            if (session == null)
                throw new InvalidOperationException("Trying to get session without beginning a transaction first. Make sure to call BeginTransaction before getting session instance.");

            return session.Value;
        }

        public bool IsQueryTransactionStarted
        {
            get { return false; }
        }

        public void Dispose()
        {
            if (this.lazyCommandSession != null)
            {
                if (this.lazyCommandSession.IsValueCreated)
                {
                    this.lazyCommandSession.Value.Dispose();
                }

                this.lazyCommandSession = null;
            }

            GC.SuppressFinalize(this);
        }

        ~NoTransactionCqrsPostgresTransactionManager()
        {
            this.Dispose();
        }
    }
}