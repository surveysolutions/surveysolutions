using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using WB.Core.Infrastructure.PlainStorage;
using ActionFilterAttribute = System.Web.Mvc.ActionFilterAttribute;

namespace WB.UI.Designer.Code
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
            TransactionManager.BeginTransaction();
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            if (!TransactionManager.IsTransactionStarted)
                return;

            if (filterContext.Exception != null)
            {
                TransactionManager.RollbackTransaction();
            }
            else
            {
                TransactionManager.CommitTransaction();
            }
        }
    }
}