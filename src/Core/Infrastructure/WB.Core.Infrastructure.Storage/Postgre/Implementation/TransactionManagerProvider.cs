using System;
using System.Transactions;
using NHibernate;
using WB.Core.Infrastructure.Transactions;

namespace WB.Core.Infrastructure.Storage.Postgre.Implementation
{
    internal class TransactionManagerProvider : ISessionProvider, ITransactionManagerProviderManager
    {
        private readonly Func<ICqrsPostgresTransactionManager> transactionManagerFactory;
        private readonly Func<ICqrsPostgresTransactionManager> noTransactionTransactionManagerFactory;
        private readonly ICqrsPostgresTransactionManager rebuildReadSideTransactionManager;
        private ICqrsPostgresTransactionManager pinnedTransactionManager;

        public TransactionManagerProvider(
            Func<CqrsPostgresTransactionManager> transactionManagerFactory,
            Func<NoTransactionCqrsPostgresTransactionManager> noTransactionTransactionManagerFactory,
            RebuildReadSideCqrsPostgresTransactionManager rebuildReadSideCqrsTransactionManager)
            : this((Func<ICqrsPostgresTransactionManager>)transactionManagerFactory, (Func<ICqrsPostgresTransactionManager>) noTransactionTransactionManagerFactory, (ICqrsPostgresTransactionManager)rebuildReadSideCqrsTransactionManager) { }

        internal TransactionManagerProvider(Func<ICqrsPostgresTransactionManager> transactionManagerFactory,
            Func<ICqrsPostgresTransactionManager> noTransactionTransactionManagerFactory,
            ICqrsPostgresTransactionManager rebuildReadSideTransactionManager)
        {
            this.transactionManagerFactory = transactionManagerFactory;
            this.rebuildReadSideTransactionManager = rebuildReadSideTransactionManager;
            this.noTransactionTransactionManagerFactory = noTransactionTransactionManagerFactory;
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
            return this.pinnedTransactionManager ?? 
                (ThreadMarkerManager.IsCurrentThreadNoTransactional() ? 
                    this.noTransactionTransactionManagerFactory.Invoke() : 
                    this.transactionManagerFactory.Invoke());
        }
    }
}