using System;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.Infrastructure.Transactions
{
    public static class TransactionManagerExtensions
    {
        public static void ExecuteInPlainTransaction(this IPlainTransactionManager transactionManager, Action action)
        {
            try
            {
                transactionManager.BeginTransaction();
                
                action.Invoke();
                
                transactionManager.CommitTransaction();
            }
            catch
            {
                transactionManager.RollbackTransaction();
                throw;
            }
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

        public static void ExecuteInQueryTransaction(this ITransactionManager transactionManager, Action action)
        {
            try
            {
                transactionManager.BeginQueryTransaction();
                
                action.Invoke();
            }
            finally
            {
                transactionManager.RollbackQueryTransaction();
            }
        }

        public static T ExecuteInQueryTransaction<T>(this ITransactionManager transactionManager, Func<T> func)
        {
            try
            {
                transactionManager.BeginQueryTransaction();

                return func.Invoke();
            }
            finally
            {
                transactionManager.RollbackQueryTransaction();
            }
        }
    }
}