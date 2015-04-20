using System;
using System.Data;
using NHibernate;
using Ninject;

namespace WB.Core.Infrastructure.Storage.Postgre.Implementation
{
    internal class RebuildReadSideCqrsPostgresTransactionManager : ICqrsPostgresTransactionManager, IDisposable
    {
        private readonly ISessionFactory sessionFactory;

        private Lazy<ISession> lazyCommandSession;

        private bool triedToBeginCommandTransaction;

        public RebuildReadSideCqrsPostgresTransactionManager([Named(PostgresReadSideModule.ReadSideSessionFactoryName)]ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        public void BeginCommandTransaction()
        {
            this.triedToBeginCommandTransaction = true;

            if (this.lazyCommandSession != null)
                throw new InvalidOperationException();

            this.lazyCommandSession = new Lazy<ISession>(() => this.sessionFactory.OpenSession());
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
            throw new NotSupportedException("Queries are not allowed during read side rebuild.");
        }

        public void RollbackQueryTransaction()
        {
            throw new NotSupportedException("Queries are not allowed during read side rebuild.");
        }

        public ISession GetSession()
        {
            if (this.lazyCommandSession == null)
                throw new InvalidOperationException("Trying to get session without beginning a transaction first. Make sure to call BeginTransaction before getting session instance.");

            return this.lazyCommandSession.Value;
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

        ~RebuildReadSideCqrsPostgresTransactionManager()
        {
            this.Dispose();
        }
    }
}