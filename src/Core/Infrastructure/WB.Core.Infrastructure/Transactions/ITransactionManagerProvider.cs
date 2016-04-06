using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.Infrastructure.Transactions
{
    public interface ITransactionManagerProvider
    {
        ITransactionManager GetTransactionManager();
    }
    public interface IPlainTransactionManagerProvider
    {
        IPlainTransactionManager GetPlainTransactionManager();
        void PinRebuildReadSideTransactionManager();
        void UnpinTransactionManager();
    }
}