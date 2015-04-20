namespace WB.Core.Infrastructure.PlainStorage
{
    public interface IPlainTransactionManager
    {
        void BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction();
    }
}