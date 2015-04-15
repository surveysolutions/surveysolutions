using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Code
{
    public class PlainApiTransactionFilter : ActionFilterAttribute, IActionFilter
    {
        private readonly IPlainTransactionManager transactionManager;

        public PlainApiTransactionFilter(IPlainTransactionManager transactionManager)
        {
            this.transactionManager = transactionManager;
        }


        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            transactionManager.BeginTransaction();
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Exception != null)
            {
                transactionManager.RollbackTransaction();
            }
            else
            {
                transactionManager.CommitTransaction();
            }
        }
    }
}