using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Microsoft.Practices.ServiceLocation;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Code
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
            TransactionManager.BeginTransaction();
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Exception != null)
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