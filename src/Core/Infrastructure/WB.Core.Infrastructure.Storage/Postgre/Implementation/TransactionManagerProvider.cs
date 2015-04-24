using System;
using System.Transactions;
using NHibernate;
using WB.Core.Infrastructure.Transactions;

namespace WB.Core.Infrastructure.Storage.Postgre.Implementation
{
    internal class TransactionManagerProvider : ISessionProvider, ITransactionManagerProviderManager
    {
        private readonly Func<ICqrsPostgresTransactionManager> transactionManagerFactory;
        private readonly ICqrsPostgresTransactionManager rebuildReadSideTransactionManager;
        private ICqrsPostgresTransactionManager pinnedTransactionManager;

        public TransactionManagerProvider(
            Func<CqrsPostgresTransactionManager> transactionManagerFactory,
            RebuildReadSideCqrsPostgresTransactionManager rebuildReadSideCqrsTransactionManager)
            : this((Func<ICqrsPostgresTransactionManager>)transactionManagerFactory, (ICqrsPostgresTransactionManager)rebuildReadSideCqrsTransactionManager) { }

        internal TransactionManagerProvider(Func<ICqrsPostgresTransactionManager> transactionManagerFactory,
            ICqrsPostgresTransactionManager rebuildReadSideTransactionManager)
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

        private ICqrsPostgresTransactionManager GetPostgresTransactionManager()
        {
            return this.pinnedTransactionManager ?? this.transactionManagerFactory.Invoke();
        }
    }
}