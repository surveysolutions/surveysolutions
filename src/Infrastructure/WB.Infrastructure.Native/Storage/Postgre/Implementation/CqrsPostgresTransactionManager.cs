using System;
using System.Data;
using NHibernate;
using Ninject;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    internal class CqrsPostgresTransactionManager : ICqrsPostgresTransactionManager, IDisposable
    {
        private readonly ISessionFactory sessionFactory;

        private Lazy<SessionHandle> lazyCommandSession;

        private bool triedToBeginCommandTransaction;

        public CqrsPostgresTransactionManager([Named(PostgresReadSideModule.ReadSideSessionFactoryName)]ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        public void BeginCommandTransaction()
        {
            this.triedToBeginCommandTransaction = true;

            if (this.lazyCommandSession != null)
                throw new InvalidOperationException();

            this.lazyCommandSession = new Lazy<SessionHandle>(() =>
            {
                var commandSession = this.sessionFactory.OpenSession();
                var commandTransaction = commandSession.BeginTransaction(IsolationLevel.RepeatableRead);

                return new SessionHandle(commandSession, commandTransaction);
            });
        }

        public void CommitCommandTransaction()
        {
            if (this.lazyCommandSession == null)
                throw new InvalidOperationException();

            if (this.lazyCommandSession.IsValueCreated)
            {
                this.lazyCommandSession.Value.Transaction.Commit();
                this.lazyCommandSession.Value.Session.Close();
            }

            this.lazyCommandSession = null;

            this.triedToBeginCommandTransaction = false;
        }

        public void RollbackCommandTransaction()
        {
            if (!this.triedToBeginCommandTransaction)
                throw new InvalidOperationException("Command transaction is not started and therefore cannot be rolled back.");

            if (this.lazyCommandSession?.IsValueCreated == true)
            {
                this.lazyCommandSession.Value.Transaction.Rollback();
                this.lazyCommandSession.Value.Session.Close();
            }

            this.lazyCommandSession = null;

            this.triedToBeginCommandTransaction = false;
        }

        public ISession GetSession()
        {
            ISession commandSession = this.lazyCommandSession?.Value.Session;

            var session = commandSession;

            if (session == null)
                throw new InvalidOperationException("Trying to get session without beginning a transaction first. Make sure to call BeginTransaction before getting session instance.");

            return session;
        }

        public bool TransactionStarted => this.lazyCommandSession != null;

        public void Dispose()
        {
            if (this.lazyCommandSession?.IsValueCreated == true)
            {
                this.lazyCommandSession.Value.Dispose();
            }

            this.lazyCommandSession = null;

            GC.SuppressFinalize(this);
        }

        ~CqrsPostgresTransactionManager()
        {
            this.Dispose();
        }
    }
}