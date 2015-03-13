using System;

namespace WB.Core.Infrastructure.Transactions
{
    public class NoTransactionTransactionManagerProvider :  ITransactionManagerProvider
    {
        private readonly ITransactionManager transactionManager;

        public NoTransactionTransactionManagerProvider(ITransactionManager transactionManager)
        {
            this.transactionManager = transactionManager;
        }

        public ITransactionManager GetTransactionManager()
        {
            return transactionManager;
        }
    }
}