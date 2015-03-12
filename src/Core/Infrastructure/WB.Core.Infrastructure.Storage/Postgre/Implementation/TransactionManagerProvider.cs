using System;
using System.Transactions;
using NHibernate;
using WB.Core.Infrastructure.Transactions;

namespace WB.Core.Infrastructure.Storage.Postgre.Implementation
{
    internal class TransactionManagerProvider : ITransactionManagerProvider, ISessionProvider
    {
        private readonly Func<PostgreTransactionManager> transactionManagerFactory;

        public TransactionManagerProvider(Func<PostgreTransactionManager> transactionManagerFactory)
        {
            this.transactionManagerFactory = transactionManagerFactory;
        }

        public ITransactionManager GetTransactionManager()
        {
            return transactionManagerFactory.Invoke();
        }

        public ISession GetSession()
        {
            return transactionManagerFactory.Invoke().GetSession();
        }
    }
}