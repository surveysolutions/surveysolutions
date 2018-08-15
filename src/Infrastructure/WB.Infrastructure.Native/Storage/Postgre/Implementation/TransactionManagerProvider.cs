using System;
using NHibernate;
using WB.Core.Infrastructure.Transactions;
using WB.Infrastructure.Native.Threading;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    internal class TransactionManagerProvider : ISessionProvider, ITransactionManagerProviderManager
    {
        private readonly Func<ICqrsPostgresTransactionManager> transactionManagerFactory;
        private readonly Func<ICqrsPostgresTransactionManager> noTransactionTransactionManagerFactory;

        public TransactionManagerProvider(
            Func<CqrsPostgresTransactionManager> transactionManagerFactory,
            Func<NoTransactionCqrsPostgresTransactionManager> noTransactionTransactionManagerFactory)
            : this((Func<ICqrsPostgresTransactionManager>)transactionManagerFactory, 
                  noTransactionTransactionManagerFactory) { }

        internal TransactionManagerProvider(
            Func<ICqrsPostgresTransactionManager> transactionManagerFactory,
            Func<ICqrsPostgresTransactionManager> noTransactionTransactionManagerFactory)
        {
            this.transactionManagerFactory = transactionManagerFactory;
            this.noTransactionTransactionManagerFactory = noTransactionTransactionManagerFactory;
        }

        public ITransactionManager GetTransactionManager() => this.GetPostgresTransactionManager();

        public ISession GetSession() => this.GetPostgresTransactionManager().GetSession();
        
        private ICqrsPostgresTransactionManager GetPostgresTransactionManager()
        {
            return ThreadMarkerManager.IsCurrentThreadNoTransactional() ? 
                    this.noTransactionTransactionManagerFactory.Invoke() : 
                    this.transactionManagerFactory.Invoke();
        }
    }
}
