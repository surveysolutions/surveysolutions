using System.Web.Mvc;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Transactions;

namespace WB.UI.Shared.Web.Filters
{
    public class TransactionFilter : ActionFilterAttribute
    {
        ITransactionManagerProvider TransactionManagerProvider => ServiceLocator.Current.GetInstance<ITransactionManagerProvider>();

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            this.TransactionManagerProvider.GetTransactionManager().BeginCommandTransaction();
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            if (TransactionManagerProvider.GetTransactionManager().TransactionStarted)
            {
                if (filterContext.Exception != null)
                {
                    TransactionManagerProvider.GetTransactionManager().RollbackCommandTransaction();
                }
                else
                {
                    TransactionManagerProvider.GetTransactionManager().CommitCommandTransaction();
                }
            }
        }
    }
}