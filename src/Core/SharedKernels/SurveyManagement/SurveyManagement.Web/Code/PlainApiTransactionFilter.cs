using System.Collections.ObjectModel;
using System.Linq;
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
            if (!ShouldDisableTransaction(actionContext))
            {
                transactionManager.BeginTransaction();
            }
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (!ShouldDisableTransaction(actionExecutedContext.ActionContext))
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

        private static bool ShouldDisableTransaction(HttpActionContext actionContext)
        {
            Collection<NoTransactionAttribute> noTransactionActionAttributes =
                actionContext.ActionDescriptor.GetCustomAttributes<NoTransactionAttribute>();
            Collection<NoTransactionAttribute> noTransactionControllerAttributes =
                actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<NoTransactionAttribute>();

            bool shouldDisableTransactionForAction = noTransactionActionAttributes.Any();
            bool shouldDisableTransactionForController = noTransactionControllerAttributes.Any();

            return shouldDisableTransactionForAction || shouldDisableTransactionForController;
        }
    }
}