namespace WB.Core.Infrastructure.Transactions
{
    public interface ITransactionManagerProviderManager
    {
        void PinRebuildReadSideTransactionManager();
        void UnpinTransactionManager();
    }
}