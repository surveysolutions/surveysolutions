namespace WB.Core.Infrastructure.Transactions
{
    public class NoTransactionTransactionManager : ITransactionManager
    {
        public void BeginCommandTransaction()
        {
        }

        public void CommitCommandTransaction()
        {
        }

        public void RollbackCommandTransaction()
        {
        }

        public void BeginQueryTransaction()
        {
        }

        public void RollbackQueryTransaction()
        {
        }

        public bool IsQueryTransactionStarted
        {
            get { return false; }
        }
    }
}