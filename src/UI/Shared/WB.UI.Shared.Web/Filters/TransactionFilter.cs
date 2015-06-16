using System.Web.Mvc;
using WB.Core.Infrastructure.Transactions;

namespace WB.UI.Shared.Web.Filters
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

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            transactionManager.GetTransactionManager().RollbackQueryTransaction();
        }
    }
}