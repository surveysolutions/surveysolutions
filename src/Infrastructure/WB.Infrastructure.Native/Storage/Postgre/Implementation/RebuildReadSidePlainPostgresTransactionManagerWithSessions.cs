using System;
using NHibernate;
using Ninject;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    internal class RebuildReadSidePlainPostgresTransactionManagerWithSessions :IPlainPostgresTransactionManager, IDisposable
    {
        private readonly ISessionFactory sessionFactory;

        private Lazy<ISession> lazyQuerySession;

        public RebuildReadSidePlainPostgresTransactionManagerWithSessions([Named(PostgresPlainStorageModule.SessionFactoryName)]ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        public void BeginTransaction()
        {
            if (this.lazyQuerySession != null)
                throw new InvalidOperationException();

            this.lazyQuerySession = new Lazy<ISession>(() => this.sessionFactory.OpenSession());
        }

        public void CommitTransaction()
        {
            throw new NotSupportedException("Plain storage is not allowed to be changed during read side rebuild.");
        }

        public void RollbackTransaction()
        {
            throw new NotSupportedException("Plain storage transaction could be rolled back only on dispose.");
        }

        public bool IsTransactionStarted => true;

        public ISession GetSession()
        {
            if (this.lazyQuerySession == null)
                this.BeginTransaction();

            return this.lazyQuerySession.Value;
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
            if (this.lazyQuerySession != null)
            {
                if (this.lazyQuerySession.IsValueCreated)
                {
                    this.lazyQuerySession.Value.Dispose();
                }

                this.lazyQuerySession = null;
            }

            GC.SuppressFinalize(this);
        }
        ~RebuildReadSidePlainPostgresTransactionManagerWithSessions()
        {
            this.Dispose();
        }
    }
}