using System.Web.Mvc;
using Ninject;
using WB.Core.Infrastructure.Storage.Postgre;
using WB.Core.Infrastructure.Transactions;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Code
{
    public class TransactionFilter : ActionFilterAttribute
    {
        private readonly ITransactionManagerProvider transactionManager;

        public TransactionFilter(ITransactionManagerProvider transactionManager)
        {
            this.transactionManager = transactionManager;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            transactionManager.GetTransactionManager().BeginQueryTransaction();
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            transactionManager.GetTransactionManager().RollbackQueryTransaction();
        }
    }
}