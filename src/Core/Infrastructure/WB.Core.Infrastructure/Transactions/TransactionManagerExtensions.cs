using System;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.Infrastructure.Transactions
{
    public static class TransactionManagerExtensions
    {
        public static void ExecuteInPlainTransaction(this IPlainTransactionManager transactionManager, Action action)
        {
            transactionManager.ExecuteInPlainTransaction(action.ToFunc());
        }

        public static void ExecuteInQueryTransaction(this ITransactionManager transactionManager, Action action)
        {
            transactionManager.ExecuteInQueryTransaction(action.ToFunc());
        }

        public static T ExecuteInPlainTransaction<T>(this IPlainTransactionManager transactionManager, Func<T> func)
        {
            try
            {
                transactionManager.BeginTransaction();
                
                T result = func.Invoke();
                
                transactionManager.CommitTransaction();

                return result;
            }
            catch
            {
                transactionManager.RollbackTransaction();
                throw;
            }
        }

        public static T ExecuteInQueryTransaction<T>(this ITransactionManager transactionManager, Func<T> func)
        {
            bool shouldStartTransaction = !transactionManager.IsQueryTransactionStarted;
            try
            {

                if (shouldStartTransaction)
                {
                    transactionManager.BeginQueryTransaction();
                }

                return func.Invoke();
            }
            finally
            {
                if (shouldStartTransaction)
                {
                    transactionManager.RollbackQueryTransaction();
                }
            }
        }
    }
}