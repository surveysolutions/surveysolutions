namespace WB.Core.Infrastructure.Transactions
{
    public interface ITransactionManagerProvider
    {
        ITransactionManager GetTransactionManager();
    }
}