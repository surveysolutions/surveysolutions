namespace WB.Core.Infrastructure.Transactions
{
    public interface ITransactionManagerProviderManager
    {
        void PinTransactionManager();
        void UnpinTransactionManager();
    }
}