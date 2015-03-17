using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Ninject;
using WB.Core.Infrastructure.Storage.Postgre;
using WB.Core.Infrastructure.Transactions;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Code
{
    public class ApiTransactionFilter : ActionFilterAttribute, IActionFilter
    {
         private readonly ITransactionManagerProvider transactionManager;

         public ApiTransactionFilter(ITransactionManagerProvider transactionManager)
        {
            this.transactionManager = transactionManager;
        }


        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (!ShouldDisableTransaction(actionContext))
            {
                this.transactionManager.GetTransactionManager().BeginQueryTransaction();
            }
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (!ShouldDisableTransaction(actionExecutedContext.ActionContext))
            {
                this.transactionManager.GetTransactionManager().RollbackQueryTransaction();
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