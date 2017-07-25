using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;

namespace WB.UI.Shared.Web.Filters
{
    public class PlainApiTransactionFilter : ActionFilterAttribute
    {
        IPlainTransactionManager TransactionManager => ServiceLocator.Current.GetInstance<IPlainTransactionManagerProvider>().GetPlainTransactionManager();

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