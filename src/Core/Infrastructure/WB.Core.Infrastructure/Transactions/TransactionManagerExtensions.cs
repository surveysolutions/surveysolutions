using System;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.Infrastructure.Transactions
{
    public static class TransactionManagerExtensions
    {
        public static void ExecuteInPlainTransaction(this IServiceLocator serviceLocator, Action action)
        {
            var plainTransactionManager = serviceLocator.GetInstance<IPlainTransactionManagerProvider>().GetPlainTransactionManager();
            plainTransactionManager.ExecuteInPlainTransaction(action);
        }

        public static void ExecuteInPlainTransaction(this IServiceLocator serviceLocator, Action<IServiceLocator> action)
        {
            var plainTransactionManager = serviceLocator.GetInstance<IPlainTransactionManagerProvider>().GetPlainTransactionManager();
            plainTransactionManager.ExecuteInPlainTransaction(() => action(serviceLocator));
        }

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
            catch(Exception)
            {
                if (shouldStartTransaction)
                {
                    try
                    {
                        transactionManager.RollbackTransaction();
                    }
                    catch
                    {
                        //suppressing rollback exception to throw original exception
                    }
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
                    try
                    {
                        transactionManager.RollbackCommandTransaction();
                    }
                    catch
                    {
                        //suppressing rollback exception to throw original exception
                    }
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
                    try
                    {
                        transactionManager.RollbackTransaction();
                    }
                    catch
                    {
                        //suppressing rollback exception to throw original exception
                    }
                }
            }
        }
    }
}
