using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using WB.Core.Infrastructure.PlainStorage;
using ActionFilterAttribute = System.Web.Mvc.ActionFilterAttribute;

namespace WB.UI.Shared.Web.Filters
{
    public class PlainTransactionFilter : ActionFilterAttribute
    {
        IPlainTransactionManager TransactionManager
        {
            get
            {
                return ServiceLocator.Current.GetInstance<IPlainTransactionManager>();
            }
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            this.TransactionManager.BeginTransaction();
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            if (!this.TransactionManager.IsTransactionStarted)
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