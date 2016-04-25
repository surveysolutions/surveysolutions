using System;
using NHibernate;
using NHibernate.Persister.Entity;
using Ninject;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    internal class RebuildReadSideCqrsPostgresTransactionManagerWithSessions : ICqrsPostgresTransactionManager, IDisposable
    {
        private readonly ISessionFactory sessionFactory;

        private Lazy<ISession> lazyCommandSession;

        private bool triedToBeginCommandTransaction;

        public RebuildReadSideCqrsPostgresTransactionManagerWithSessions([Named(PostgresReadSideModule.ReadSideSessionFactoryName)]ISessionFactory sessionFactory)
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

        public string GetEntityIdentifierColumnName(Type entityType)
        {
            var persister = this.sessionFactory.GetClassMetadata(entityType);

            if (persister == null)
                return null;

            return persister.IdentifierPropertyName;
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

        ~RebuildReadSideCqrsPostgresTransactionManagerWithSessions()
        {
            this.Dispose();
        }
    }
}