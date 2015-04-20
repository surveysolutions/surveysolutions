using System;
using System.Transactions;
using NHibernate;
using WB.Core.Infrastructure.Transactions;

namespace WB.Core.Infrastructure.Storage.Postgre.Implementation
{
    internal class TransactionManagerProvider : ISessionProvider, ITransactionManagerProviderManager
    {
        private readonly Func<CqrsPostgresTransactionManager> transactionManagerFactory;
        private readonly RebuildReadSideCqrsPostgresTransactionManager rebuildReadSideCqrsTransactionManager;
        private ICqrsPostgresTransactionManager pinnedTransactionManager;

        public TransactionManagerProvider(Func<CqrsPostgresTransactionManager> transactionManagerFactory,
            RebuildReadSideCqrsPostgresTransactionManager rebuildReadSideCqrsTransactionManager)
        {
            this.transactionManagerFactory = transactionManagerFactory;
            this.rebuildReadSideCqrsTransactionManager = rebuildReadSideCqrsTransactionManager;
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
            this.pinnedTransactionManager = this.rebuildReadSideCqrsTransactionManager;
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