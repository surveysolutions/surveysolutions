namespace WB.Core.Infrastructure.Transactions
{
    public interface ITransactionManagerProviderManager : ITransactionManagerProvider
    {
        void PinRebuildReadSideTransactionManager();
        void UnpinTransactionManager();
    }
}