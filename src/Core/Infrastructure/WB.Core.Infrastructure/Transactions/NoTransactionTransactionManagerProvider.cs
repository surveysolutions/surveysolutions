using System;

namespace WB.Core.Infrastructure.Transactions
{
    public class NoTransactionTransactionManagerProvider : ITransactionManagerProvider, ITransactionManagerProviderManager
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

        public void PinRebuildReadSideTransactionManager() {}

        public void UnpinTransactionManager() {}
    }
}