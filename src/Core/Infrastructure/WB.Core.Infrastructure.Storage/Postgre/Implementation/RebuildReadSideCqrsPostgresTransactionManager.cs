using System;
using System.Data;
using NHibernate;

namespace WB.Core.Infrastructure.Storage.Postgre.Implementation
{
    internal class RebuildReadSideCqrsPostgresTransactionManager : ICqrsPostgresTransactionManager, IDisposable
    {
        private readonly ISessionFactory sessionFactory;

        private ISession commandSession;

        private bool triedToBeginCommandTransaction;

        public RebuildReadSideCqrsPostgresTransactionManager(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        public void BeginCommandTransaction()
        {
            this.triedToBeginCommandTransaction = true;

            if (this.commandSession != null)
                throw new InvalidOperationException();

            this.commandSession = this.sessionFactory.OpenSession();
        }

        public void CommitCommandTransaction()
        {
            if (this.commandSession == null)
                throw new InvalidOperationException();

            this.commandSession.Flush();
            this.commandSession.Close();
            this.commandSession = null;

            this.triedToBeginCommandTransaction = false;
        }

        public void RollbackCommandTransaction()
        {
            if (!this.triedToBeginCommandTransaction)
                throw new InvalidOperationException();

            if (this.commandSession != null)
            {
                this.commandSession.Close();
                this.commandSession = null;
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
            if (this.commandSession == null)
                throw new InvalidOperationException("Trying to get session without beginning a transaction first. Make sure to call BeginTransaction before getting session instance.");

            return this.commandSession;
        }

        public bool IsQueryTransactionStarted
        {
            get { return false; }
        }

        public void Dispose()
        {
            if (this.commandSession != null)
            {
                this.commandSession.Dispose();
                this.commandSession = null;
            }

            GC.SuppressFinalize(this);
        }

        ~RebuildReadSideCqrsPostgresTransactionManager()
        {
            this.Dispose();
        }
    }
}