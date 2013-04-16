using System;
#if !MONODROID
using System.Transactions;
#endif
namespace Ncqrs.Commanding.CommandExecution
{
    public class DefaultTransactionService : ITransactionService
    {
        #if !MONODROID
        private readonly TransactionOptions _options;        

        public DefaultTransactionService()
        {
            ScopeOption = TransactionScopeOption.Required;
            _options = new TransactionOptions
                           {
                               IsolationLevel = IsolationLevel.ReadCommitted, 
                               Timeout = TransactionManager.MaximumTimeout
                           };
        }

        public DefaultTransactionService(TransactionOptions options)
        {
            ScopeOption = TransactionScopeOption.Required;
            _options = options;
        }


        public TransactionOptions Options
        {
            get { return _options; }
        }

        public TransactionScopeOption ScopeOption { get; set; }
#endif
        public void ExecuteInTransaction(Action action)
        { 
#if !MONODROID
            using (var scope = new TransactionScope(ScopeOption, Options))
            {
#endif
                action();
            #if !MONODROID
                scope.Complete();
            }
#endif
        }
    }
}