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
            this.transactionManager.GetTransactionManager().BeginQueryTransaction();
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            this.transactionManager.GetTransactionManager().RollbackQueryTransaction();
        }
    }
}