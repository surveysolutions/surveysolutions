using System.Collections.ObjectModel;
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
            if (HasTransaction(actionContext))
            {
                this.transactionManager.GetTransactionManager().BeginQueryTransaction();
            }
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (HasTransaction(actionExecutedContext.ActionContext))
            {
                this.transactionManager.GetTransactionManager().RollbackQueryTransaction();
            }
        }

        private static bool HasTransaction(HttpActionContext actionContext)
        {
            Collection<NoTransactionAttribute> noTransactionAttributes =
                actionContext.ActionDescriptor.GetCustomAttributes<NoTransactionAttribute>();
            bool hasTransaction = noTransactionAttributes.Count == 0;
            return hasTransaction;
        }
    }
}