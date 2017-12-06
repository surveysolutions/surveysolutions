using System.Web.Mvc;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using ActionFilterAttribute = System.Web.Mvc.ActionFilterAttribute;

namespace WB.UI.Shared.Web.Filters
{
    public class PlainTransactionFilter : ActionFilterAttribute
    {
        IPlainTransactionManager TransactionManager => ServiceLocator.Current.GetInstance<IPlainTransactionManagerProvider>().GetPlainTransactionManager();

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            this.TransactionManager.BeginTransaction();
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            if (!this.TransactionManager.TransactionStarted)
                return;

            if (filterContext.Exception != null)
            {
                this.TransactionManager.RollbackTransaction();
            }
            else
            {
                this.TransactionManager.CommitTransaction();
            }
        }
    }
}