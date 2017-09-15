using System;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.Infrastructure.Transactions
{
    public static class TransactionManagerExtensions
    {
        public static void ExecuteInPlainTransaction(this IPlainTransactionManager transactionManager, Action action)
        {
            transactionManager.ExecuteInPlainTransaction(action.ToFunc());
        }

        public static void ExecuteInQueryTransaction(this IPlainTransactionManager transactionManager, Action action)
        {
            transactionManager.ExecuteInQueryTransaction(action.ToFunc());
        }

        public static void ExecuteInQueryTransaction(this ITransactionManager transactionManager, Action action)
        {
            transactionManager.ExecuteInQueryTransaction(action.ToFunc());
        }

        public static T ExecuteInPlainTransaction<T>(this IPlainTransactionManager transactionManager, Func<T> func)
        {
            bool shouldStartTransaction = !transactionManager.TransactionStarted;
            try
            {
                if (shouldStartTransaction)
                {
                    transactionManager.BeginTransaction();
                }

                T result = func.Invoke();
                if (shouldStartTransaction)
                {
                    transactionManager.CommitTransaction();
                }
                return result;
            }
            catch(Exception r)
            {
                if (shouldStartTransaction)
                {
                    transactionManager.RollbackTransaction();
                }
                throw;
            }
        }

        public static T ExecuteInQueryTransaction<T>(this ITransactionManager transactionManager, Func<T> func)
        {
            bool shouldStartTransaction = !transactionManager.TransactionStarted;
            try
            {
                if (shouldStartTransaction)
                {
                    transactionManager.BeginCommandTransaction();
                }

                return func.Invoke();
            }
            finally
            {
                if (shouldStartTransaction)
                {
                    transactionManager.RollbackCommandTransaction();
                }
            }
        }

        public static T ExecuteInQueryTransaction<T>(this IPlainTransactionManager transactionManager, Func<T> func)
        {
            bool shouldStartTransaction = !transactionManager.TransactionStarted;
            try
            {

                if (shouldStartTransaction)
                {
                    transactionManager.BeginTransaction();
                }

                return func.Invoke();
            }
            finally
            {
                if (shouldStartTransaction)
                {
                    transactionManager.RollbackTransaction();
                }
            }
        }
    }
}