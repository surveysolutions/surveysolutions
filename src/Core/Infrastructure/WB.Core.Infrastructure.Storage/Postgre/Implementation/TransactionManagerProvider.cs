using System;
using System.Transactions;
using NHibernate;
using WB.Core.Infrastructure.Transactions;

namespace WB.Core.Infrastructure.Storage.Postgre.Implementation
{
    internal class TransactionManagerProvider : ITransactionManagerProvider, ISessionProvider, ITransactionManagerProviderManager
    {
        private readonly Func<PostgreTransactionManager> transactionManagerFactory;
        private PostgreTransactionManager pinnedTransactionManager;

        public TransactionManagerProvider(Func<PostgreTransactionManager> transactionManagerFactory)
        {
            this.transactionManagerFactory = transactionManagerFactory;
        }

        public ITransactionManager GetTransactionManager()
        {
            return this.GetPostgreTransactionManager();
        }

        public ISession GetSession()
        {
            return this.GetPostgreTransactionManager().GetSession();
        }

        public void PinTransactionManager()
        {
            this.pinnedTransactionManager = this.transactionManagerFactory.Invoke();
        }

        public void UnpinTransactionManager()
        {
            this.pinnedTransactionManager.Dispose();
            this.pinnedTransactionManager = null;
        }

        private PostgreTransactionManager GetPostgreTransactionManager()
        {
            return this.pinnedTransactionManager ?? this.transactionManagerFactory.Invoke();
        }
    }
}