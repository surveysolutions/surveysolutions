using System;

namespace Ncqrs.Commanding.CommandExecution
{
    public class DefaultTransactionService : ITransactionService
    {
        public void ExecuteInTransaction(Action action)
        { 
            action();
        }
    }
}