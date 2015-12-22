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
        private readonly ICqrsPostgresTransactionManager rebuildReadSideTransactionManagerWithSessions;
        private readonly ICqrsPostgresTransactionManager rebuildReadSideTransactionManagerWithoutSessions;

        private ICqrsPostgresTransactionManager pinnedTransactionManager;
        private readonly ReadSideCacheSettings cacheSettings;

        public TransactionManagerProvider(
            Func<CqrsPostgresTransactionManager> transactionManagerFactory,
            Func<NoTransactionCqrsPostgresTransactionManager> noTransactionTransactionManagerFactory,
            RebuildReadSideCqrsPostgresTransactionManagerWithSessions rebuildReadSideCqrsPostgresTransactionManagerWithSessions,
            RebuildReadSideCqrsPostgresTransactionManagerWithoutSessions rebuildReadSideCqrsPostgresTransactionManagerWithoutSessions,
            ReadSideCacheSettings cacheSettings)
            : this(transactionManagerFactory, 
                  noTransactionTransactionManagerFactory,
                  (ICqrsPostgresTransactionManager)rebuildReadSideCqrsPostgresTransactionManagerWithSessions,
                  (ICqrsPostgresTransactionManager)rebuildReadSideCqrsPostgresTransactionManagerWithoutSessions,
                  cacheSettings) { }

        internal TransactionManagerProvider(
            Func<ICqrsPostgresTransactionManager> transactionManagerFactory,
            Func<ICqrsPostgresTransactionManager> noTransactionTransactionManagerFactory,
            ICqrsPostgresTransactionManager rebuildReadSideTransactionManagerWithSessions,
            ICqrsPostgresTransactionManager rebuildReadSideTransactionManagerWithoutSessions,
            ReadSideCacheSettings cacheSettings)
        {
            this.transactionManagerFactory = transactionManagerFactory;
            this.noTransactionTransactionManagerFactory = noTransactionTransactionManagerFactory;
            this.rebuildReadSideTransactionManagerWithSessions = rebuildReadSideTransactionManagerWithSessions;
            this.rebuildReadSideTransactionManagerWithoutSessions = rebuildReadSideTransactionManagerWithoutSessions;
            this.cacheSettings = cacheSettings;
        }

        public ITransactionManager GetTransactionManager() => this.GetPostgresTransactionManager();

        public ISession GetSession() => this.GetPostgresTransactionManager().GetSession();

        public void PinRebuildReadSideTransactionManager()
        {
            this.pinnedTransactionManager = this.ShouldDisableSessions() ? this.rebuildReadSideTransactionManagerWithoutSessions : this.rebuildReadSideTransactionManagerWithSessions;
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

        private bool ShouldDisableSessions() => this.cacheSettings.EnableEsentCache;
    }
}