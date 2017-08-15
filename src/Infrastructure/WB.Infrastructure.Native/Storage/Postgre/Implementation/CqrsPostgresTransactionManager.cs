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
        private Lazy<SessionHandle> lazyQuerySession;

        private bool triedToBeginCommandTransaction;
        private bool triedToBeginQueryTransaction;

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
                try
                {
                    this.lazyCommandSession.Value.Transaction.Commit();
                }
                finally
                {
                    this.lazyCommandSession.Value.Session.Close();
                }
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
                try
                {
                    this.lazyCommandSession.Value.Transaction.Rollback();
                }
                finally
                {
                    this.lazyCommandSession.Value.Session.Close();
                }
            }

            this.lazyCommandSession = null;

            this.triedToBeginCommandTransaction = false;
        }

        public void BeginQueryTransaction()
        {
            this.triedToBeginQueryTransaction = true;

            if (this.lazyQuerySession != null)
                throw new InvalidOperationException("Query transaction is already started");

            if (this.lazyCommandSession != null)
                throw new InvalidOperationException("Query transaction is expected to be always open before CommandTransaction, or not opened at all for this request. Please make sure that this controller has action filter for transactions management applied. But some controllers like RebuildReadSide should not ever open query transaction. Check that you are not inside such controller before fixing any code.");

            this.lazyQuerySession = new Lazy<SessionHandle>(() =>
            {
                var querySession = this.sessionFactory.OpenSession();
                var queryTransaction = querySession.BeginTransaction(IsolationLevel.ReadCommitted);

                return new SessionHandle(querySession, queryTransaction);
            });
        }

        public void RollbackQueryTransaction()
        {
            if (!this.triedToBeginQueryTransaction)
                throw new InvalidOperationException("Query transaction is not started and therefore cannot be rolled back.");

            if (this.lazyQuerySession?.IsValueCreated == true)
            {
                try
                {
                    this.lazyQuerySession.Value.Transaction.Rollback();
                }
                finally
                {
                    this.lazyQuerySession.Value.Session.Close();
                }
            }

            this.lazyQuerySession = null;

            this.triedToBeginQueryTransaction = false;
        }

        public ISession GetSession()
        {
            ISession querySession = this.lazyQuerySession?.Value.Session;
            ISession commandSession = this.lazyCommandSession?.Value.Session;

            var session = commandSession ?? querySession;

            if (session == null)
                throw new InvalidOperationException("Trying to get session without beginning a transaction first. Make sure to call BeginTransaction before getting session instance.");

            return session;
        }

        public bool IsQueryTransactionStarted => this.lazyQuerySession != null;

        public void Dispose()
        {
            if (this.lazyCommandSession?.IsValueCreated == true)
            {
                this.lazyCommandSession.Value.Dispose();
            }

            this.lazyCommandSession = null;

            if (this.lazyQuerySession?.IsValueCreated == true)
            {
                this.lazyQuerySession.Value.Dispose();
            }

            this.lazyQuerySession = null;

            GC.SuppressFinalize(this);
        }

        ~CqrsPostgresTransactionManager()
        {
            this.Dispose();
        }
    }
}