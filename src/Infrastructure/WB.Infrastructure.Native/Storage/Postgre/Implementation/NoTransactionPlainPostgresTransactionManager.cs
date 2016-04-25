using System;
using NHibernate;
using Ninject;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    internal class NoTransactionPlainPostgresTransactionManager : IPlainPostgresTransactionManager, IDisposable
    {
        private readonly ISessionFactory sessionFactory;

        private Lazy<ISession> lazyCommandSession;

        private bool triedToBeginCommandTransaction;

        public NoTransactionPlainPostgresTransactionManager([Named(PostgresPlainStorageModule.SessionFactoryName)]ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        public void BeginTransaction()
        {
            this.triedToBeginCommandTransaction = true;

            if (this.lazyCommandSession != null)
                throw new InvalidOperationException();

            this.lazyCommandSession = new Lazy<ISession>(() => this.sessionFactory.OpenSession(), true);
        }

        public void CommitTransaction()
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

        public void RollbackTransaction()
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

        public bool IsTransactionStarted => false;

        public ISession GetSession()
        {
            var session = this.lazyCommandSession;
            if (session == null)
                throw new InvalidOperationException("Trying to get session without beginning a transaction first. Make sure to call BeginTransaction before getting session instance.");

            return session.Value;
        }

        public string GetEntityIdentifierColumnName(Type entityType)
        {
            var persister = this.sessionFactory.GetClassMetadata(entityType);

            if (persister == null)
                return null;

            return persister.IdentifierPropertyName;
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

        ~NoTransactionPlainPostgresTransactionManager()
        {
            this.Dispose();
        }
    }
}