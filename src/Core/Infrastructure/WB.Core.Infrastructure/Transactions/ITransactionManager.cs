namespace WB.Core.Infrastructure.Transactions
{
    public interface ITransactionManager
    {
        void BeginCommandTransaction();
        void CommitCommandTransaction();
        void RollbackCommandTransaction();

        bool TransactionStarted { get; }
    }
}