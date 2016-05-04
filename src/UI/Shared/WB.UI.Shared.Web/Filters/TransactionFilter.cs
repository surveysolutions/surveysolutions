using System.Web.Mvc;
using WB.Core.Infrastructure.Transactions;

namespace WB.UI.Shared.Web.Filters
{
    public class TransactionFilter : ActionFilterAttribute
    {
        private readonly ITransactionManagerProvider transactionManagerProvider;

        public TransactionFilter(ITransactionManagerProvider transactionManagerProvider)
        {
            this.transactionManagerProvider = transactionManagerProvider;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            this.transactionManagerProvider.GetTransactionManager().BeginQueryTransaction();
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            var transactionManager = this.transactionManagerProvider.GetTransactionManager();
            if (transactionManager.IsQueryTransactionStarted)
                transactionManager.RollbackQueryTransaction();
        }
    }
}