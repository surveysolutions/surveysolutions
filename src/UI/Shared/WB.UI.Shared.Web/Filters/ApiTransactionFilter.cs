using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Transactions;

namespace WB.UI.Shared.Web.Filters
{
    public class ApiTransactionFilter : ActionFilterAttribute
    {
        ITransactionManagerProvider TransactionManager => ServiceLocator.Current.GetInstance<ITransactionManagerProvider>();
        
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            this.TransactionManager.GetTransactionManager().BeginQueryTransaction();
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            this.TransactionManager.GetTransactionManager().RollbackQueryTransaction();
        }
    }
}