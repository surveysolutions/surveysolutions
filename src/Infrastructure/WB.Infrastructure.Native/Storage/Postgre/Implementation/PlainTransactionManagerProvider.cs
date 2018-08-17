using System;
using NHibernate;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Infrastructure.Native.Threading;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    internal class PlainTransactionManagerProvider : IPlainSessionProvider
    {
        private readonly Func<IPlainPostgresTransactionManager> transactionManagerFactory;
        private readonly Func<IPlainPostgresTransactionManager> noTransactionTransactionManagerFactory;

        public PlainTransactionManagerProvider(
            Func<PlainPostgresTransactionManager> transactionManagerFactory,
            Func<NoTransactionPlainPostgresTransactionManager> noTransactionTransactionManagerFactory
            )
            : this((Func<IPlainPostgresTransactionManager>)transactionManagerFactory,
                noTransactionTransactionManagerFactory
                )
        { }

        internal PlainTransactionManagerProvider(
            Func<IPlainPostgresTransactionManager> transactionManagerFactory,
            Func<IPlainPostgresTransactionManager> noTransactionTransactionManagerFactory)
        {
            this.transactionManagerFactory = transactionManagerFactory;
            this.noTransactionTransactionManagerFactory = noTransactionTransactionManagerFactory;
        }

        public IPlainTransactionManager GetPlainTransactionManager() => this.GetPostgresTransactionManager();

        public ISession GetSession() => this.GetPostgresTransactionManager().GetSession();

        private IPlainPostgresTransactionManager GetPostgresTransactionManager()
        {
            return ThreadMarkerManager.IsCurrentThreadNoTransactional() ?
                this.noTransactionTransactionManagerFactory.Invoke() :
                this.transactionManagerFactory.Invoke();
        }
    }
}
