using System;
using System.Transactions;
using NHibernate;
using WB.Core.Infrastructure.Transactions;
using IsolationLevel = System.Data.IsolationLevel;

namespace WB.Core.Infrastructure.Storage.Postgre.Implementation
{
    internal class PostgreTransactionManager : ITransactionManager, ISessionProvider, IDisposable
    {
        private readonly ISessionFactory sessionFactory;

        //private ISession session;
        private ITransaction commandTransaction;
        private ITransaction queryTransaction;
        private ISession commandSession;
        private ISession querySession;

        public PostgreTransactionManager(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        public void BeginCommandTransaction()
        {
            if (this.commandTransaction != null) throw new InvalidOperationException();

            this.commandSession = this.sessionFactory.OpenSession();


            this.commandTransaction = commandSession.BeginTransaction(IsolationLevel.ReadCommitted);
        }

        public void CommitCommandTransaction()
        {
            if (this.commandTransaction == null) throw new InvalidOperationException();

            this.commandTransaction.Commit();

            this.commandTransaction.Dispose();
            this.commandSession.Close();
            this.commandTransaction = null;
            this.commandSession = null;
        }

        public void RollbackCommandTransaction()
        {
            if (this.commandTransaction == null) throw new InvalidOperationException();

            this.commandTransaction.Rollback();
            this.commandSession.Close();

            this.commandTransaction = null;
            this.commandSession = null;
        }

        public void BeginQueryTransaction()
        {
            if (this.queryTransaction != null) throw new InvalidOperationException();
            if (this.commandTransaction != null) throw new InvalidOperationException("Query transaction is expected to be always open before CommandTransaction, or not openned at all for this request. Please make sure that this controller has action filter for transactions management applied. But some controllers like RebuildReadSide should not ever open query transaction. Check that you are not inside such controller before fixing any code.");

            this.querySession = this.sessionFactory.OpenSession();

            this.queryTransaction = this.querySession.BeginTransaction(IsolationLevel.ReadCommitted);
        }

        public void RollbackQueryTransaction()
        {
            if (this.queryTransaction == null) throw new InvalidOperationException();

            this.queryTransaction.Rollback();
            this.queryTransaction = null;

            this.querySession.Close();
            this.querySession = null;
        }

        public ISession GetSession()
        {
            var result = this.commandSession ?? this.querySession;

            if (result == null) throw new InvalidOperationException("Trying to get session without beginning a transaction first. Make sure to call BeginTransaction before getting session instance");
            return result;
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

        ~PostgreTransactionManager()
        {
            Dispose();
        }
    }
}