using System.Web.Mvc;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Code
{
    public class PlainTransactionFilter : ActionFilterAttribute
    {
        private readonly IPlainTransactionManager transactionManager;

        public PlainTransactionFilter(IPlainTransactionManager transactionManager)
        {
            this.transactionManager = transactionManager;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            transactionManager.BeginTransaction();
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            if (filterContext.Exception != null)
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