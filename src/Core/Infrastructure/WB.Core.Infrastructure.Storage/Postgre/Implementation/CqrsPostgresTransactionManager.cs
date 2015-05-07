using System;
using System.Data;
using NHibernate;
using Ninject;

namespace WB.Core.Infrastructure.Storage.Postgre.Implementation
{
    internal class CqrsPostgresTransactionManager : ICqrsPostgresTransactionManager, IDisposable
    {
        private readonly ISessionFactory sessionFactory;

        private ITransaction commandTransaction;
        private ITransaction queryTransaction;
        private ISession commandSession;
        private ISession querySession;

        private bool triedToBeginCommandTransaction;
        private bool triedToBeginQueryTransaction;

        public CqrsPostgresTransactionManager([Named(PostgresReadSideModule.ReadSideSessionFactoryName)]ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        public void BeginCommandTransaction()
        {
            this.triedToBeginCommandTransaction = true;

            if (this.commandTransaction != null && this.commandSession != null)
                throw new InvalidOperationException();

            this.commandSession = this.sessionFactory.OpenSession();
            this.commandTransaction = commandSession.BeginTransaction(IsolationLevel.RepeatableRead);
        }

        public void CommitCommandTransaction()
        {
            if (this.commandTransaction == null || this.commandSession == null)
                throw new InvalidOperationException();

            this.commandTransaction.Commit();
            this.commandTransaction = null;

            this.commandSession.Close();
            this.commandSession = null;

            this.triedToBeginCommandTransaction = false;
        }

        public void RollbackCommandTransaction()
        {
            if (!this.triedToBeginCommandTransaction)
                throw new InvalidOperationException();

            if (this.commandTransaction != null)
            {
                this.commandTransaction.Rollback();
                this.commandTransaction = null;
            }

            if (this.commandSession != null)
            {
                this.commandSession.Close();
                this.commandSession = null;
            }

            this.triedToBeginCommandTransaction = false;
        }

        public void BeginQueryTransaction()
        {
            this.triedToBeginQueryTransaction = true;

            if (this.queryTransaction != null && this.querySession != null)
                throw new InvalidOperationException("Query transaction is already started");

            if (this.commandTransaction != null)
                throw new InvalidOperationException("Query transaction is expected to be always open before CommandTransaction, or not opened at all for this request. Please make sure that this controller has action filter for transactions management applied. But some controllers like RebuildReadSide should not ever open query transaction. Check that you are not inside such controller before fixing any code.");

            this.querySession = this.sessionFactory.OpenSession();
            this.queryTransaction = this.querySession.BeginTransaction(IsolationLevel.ReadCommitted);
        }

        public void RollbackQueryTransaction()
        {
            if (!this.triedToBeginQueryTransaction)
                throw new InvalidOperationException();

            if (this.queryTransaction != null)
            {
                this.queryTransaction.Rollback();
                this.queryTransaction = null;
            }

            if (this.querySession != null)
            {
                this.querySession.Close();
                this.querySession = null;
            }

            this.triedToBeginQueryTransaction = false;
        }

        public ISession GetSession()
        {
            var session = this.commandSession ?? this.querySession;

            if (session == null)
                throw new InvalidOperationException("Trying to get session without beginning a transaction first. Make sure to call BeginTransaction before getting session instance.");

            return session;
        }

        public bool IsQueryTransactionStarted
        {
            get { return this.queryTransaction != null; }
        }

        public void Dispose()
        {
            if (this.commandSession != null)
            {
                this.commandSession.Dispose();
                this.commandSession = null;
            }

            if (this.querySession != null)
            {
                this.querySession.Dispose();
                this.querySession = null;
            }

            if (this.commandTransaction != null)
            {
                this.commandTransaction.Dispose();
                this.commandTransaction = null;
            }

            if (this.queryTransaction != null)
            {
                this.queryTransaction.Dispose();
                this.queryTransaction = null;
            }

            GC.SuppressFinalize(this);
        }

        ~CqrsPostgresTransactionManager()
        {
            Dispose();
        }
    }
}