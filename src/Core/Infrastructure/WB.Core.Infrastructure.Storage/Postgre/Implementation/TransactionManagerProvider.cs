using System;
using System.Transactions;
using NHibernate;
using WB.Core.Infrastructure.Transactions;

namespace WB.Core.Infrastructure.Storage.Postgre.Implementation
{
    internal class TransactionManagerProvider : ISessionProvider, ITransactionManagerProviderManager
    {
        private readonly Func<PostgresTransactionManager> transactionManagerFactory;
        private readonly RebuildReadSidePostgresTransactionManager rebuildReadSideTransactionManager;
        private IPostgresTransactionManager pinnedTransactionManager;

        public TransactionManagerProvider(Func<PostgresTransactionManager> transactionManagerFactory,
            RebuildReadSidePostgresTransactionManager rebuildReadSideTransactionManager)
        {
            this.transactionManagerFactory = transactionManagerFactory;
            this.rebuildReadSideTransactionManager = rebuildReadSideTransactionManager;
        }

        public ITransactionManager GetTransactionManager()
        {
            return this.GetPostgresTransactionManager();
        }

        public ISession GetSession()
        {
            return this.GetPostgresTransactionManager().GetSession();
        }

        public void PinRebuildReadSideTransactionManager()
        {
            this.pinnedTransactionManager = this.rebuildReadSideTransactionManager;
        }

        public void UnpinTransactionManager()
        {
            this.pinnedTransactionManager = null;
        }

        private IPostgresTransactionManager GetPostgresTransactionManager()
        {
            return this.pinnedTransactionManager ?? this.transactionManagerFactory.Invoke();
        }
    }
}