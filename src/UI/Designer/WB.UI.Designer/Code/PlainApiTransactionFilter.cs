using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Microsoft.Practices.ServiceLocation;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.UI.Designer.Code
{
    public class PlainApiTransactionFilter : ActionFilterAttribute, IActionFilter
    {
        IPlainTransactionManager TransactionManager
        {
            get
            {
                return ServiceLocator.Current.GetInstance<IPlainTransactionManager>();
            }
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            this.TransactionManager.BeginTransaction();
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Exception != null)
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