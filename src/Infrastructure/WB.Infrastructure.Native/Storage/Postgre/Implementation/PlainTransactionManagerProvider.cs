using System;
using NHibernate;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Infrastructure.Native.Threading;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    internal class PlainTransactionManagerProvider : IPlainSessionProvider, IPlainTransactionManagerProvider
    {
        private readonly Func<IPlainPostgresTransactionManager> transactionManagerFactory;
        private readonly Func<IPlainPostgresTransactionManager> noTransactionTransactionManagerFactory;
        private readonly IPlainPostgresTransactionManager rebuildReadSideTransactionManagerWithSessions;

        private IPlainPostgresTransactionManager pinnedTransactionManager;

        public PlainTransactionManagerProvider(
            Func<PlainPostgresTransactionManager> transactionManagerFactory,
            Func<NoTransactionPlainPostgresTransactionManager> noTransactionTransactionManagerFactory,
            RebuildReadSidePlainPostgresTransactionManagerWithSessions rebuildReadSideCqrsPostgresTransactionManagerWithSessions)
            : this(transactionManagerFactory,
                noTransactionTransactionManagerFactory,
                (IPlainPostgresTransactionManager)rebuildReadSideCqrsPostgresTransactionManagerWithSessions)
        { }

        internal PlainTransactionManagerProvider(
            Func<IPlainPostgresTransactionManager> transactionManagerFactory,
            Func<IPlainPostgresTransactionManager> noTransactionTransactionManagerFactory,
            IPlainPostgresTransactionManager rebuildReadSideTransactionManagerWithSessions)
        {
            this.transactionManagerFactory = transactionManagerFactory;
            this.noTransactionTransactionManagerFactory = noTransactionTransactionManagerFactory;
            this.rebuildReadSideTransactionManagerWithSessions = rebuildReadSideTransactionManagerWithSessions;
        }

        public IPlainTransactionManager GetPlainTransactionManager() => this.GetPostgresTransactionManager();

        public ISession GetSession() => this.GetPostgresTransactionManager().GetSession();

        public void PinRebuildReadSideTransactionManager()
        {
            this.pinnedTransactionManager = this.rebuildReadSideTransactionManagerWithSessions;
        }

        public void UnpinTransactionManager()
        {
            var pinnedTransactionDisposable = this.rebuildReadSideTransactionManagerWithSessions as IDisposable;
            pinnedTransactionDisposable?.Dispose();

            this.pinnedTransactionManager = null;
        }

        private IPlainPostgresTransactionManager GetPostgresTransactionManager()
        {
            return this.pinnedTransactionManager ??
                   (ThreadMarkerManager.IsCurrentThreadNoTransactional() ?
                       this.noTransactionTransactionManagerFactory.Invoke() :
                       this.transactionManagerFactory.Invoke());
        }
    }
}