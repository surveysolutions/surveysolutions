using System;
using System.Data;
using NHibernate;
using Ninject;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    public class PlainPostgresTransactionManager : IPlainPostgresTransactionManager, IPlainSessionProvider, IDisposable
    {
        private readonly ISessionFactory sessionFactory;
        private Lazy<SessionHandle> lazySession;

        public PlainPostgresTransactionManager([Named(PostgresPlainStorageModule.SessionFactoryName)]ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        public void BeginTransaction()
        {
            if (this.lazySession != null)
                throw new InvalidOperationException("Session/Transaction already started for this instance");

            this.lazySession = new Lazy<SessionHandle>(() =>
            {
                var session = this.sessionFactory.OpenSession();
                var transaction = session.BeginTransaction(IsolationLevel.ReadCommitted);

                return new SessionHandle(session, transaction);
            });
        }

        public void CommitTransaction()
        {
            if (this.lazySession == null)
                throw new InvalidOperationException("Trying to commit transaction without beginning it");

            if (this.lazySession.IsValueCreated)
            {
                try
                {
                    this.lazySession.Value.Transaction.Commit();
                }
                finally
                {
                    this.lazySession.Value.Session.Close();
                }
            }

            this.lazySession = null;
        }


        public void RollbackTransaction()
        {
            if (this.lazySession == null)
                throw new InvalidOperationException("Trying to rollback transaction without beginning it");

            if (this.lazySession.IsValueCreated)
            {
                try
                {
                    this.lazySession.Value.Transaction.Rollback();
                }
                finally
                {
                    this.lazySession.Value.Session.Close();
                }
            }

            this.lazySession = null;
        }

        public bool IsTransactionStarted => this.lazySession != null;

        public void Dispose()
        {
            if (this.lazySession?.IsValueCreated == true)
            {
                this.lazySession.Value.Dispose();
            }

            this.lazySession = null;
        }

        public ISession GetSession()
        {
            if (this.lazySession == null)
                throw new InvalidOperationException("Trying to get session instance without starting a transaction first. Call BeginTransaction before getting session instance");

            return this.lazySession.Value.Session;
        }
    }
}