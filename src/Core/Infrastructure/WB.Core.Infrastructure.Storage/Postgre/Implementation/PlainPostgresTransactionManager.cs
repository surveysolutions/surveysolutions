using System;
using System.Data;
using NHibernate;
using Ninject;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.Infrastructure.Storage.Postgre.Implementation
{
    public class PlainPostgresTransactionManager : IPlainTransactionManager, IPlainSessionProvider, IDisposable
    {
        private readonly ISessionFactory sessionFactory;
        private ITransaction transaction;
        private ISession session;

        public PlainPostgresTransactionManager([Named(PostgresPlainStorageModule.SessionFactoryName)]ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        public void BeginTransaction()
        {
            if (this.session != null)
            {
                throw new InvalidOperationException("Session/Transaction already started for this instance");
            }

            this.session = this.sessionFactory.OpenSession();
            this.transaction = this.session.BeginTransaction(IsolationLevel.ReadCommitted);
        }

        public void CommitTransaction()
        {
            if (this.transaction == null)
            {
                throw new InvalidOperationException("Trying to commit transaction without beginning it");
            }

            this.transaction.Commit();
            this.session.Close();
            this.session = null;
        }

        public void RollbackTransaction()
        {
            if (this.transaction == null)
            {
                throw new InvalidOperationException("Trying to rollback transaction without beginning it");
            }

            this.transaction.Rollback();
            this.session.Close();
            this.session = null;
        }

        public void Dispose()
        {
            if (this.session != null)
            {
                this.session.Dispose();
                this.session = null;
            }
        }

        public ISession GetSession()
        {
            if (this.session == null)
            {
                throw new InvalidOperationException("Trying to get session istance without starting a transaction first. Call BeginTransaction before getting session instance");
            }
            return this.session; 
        }
    }
}