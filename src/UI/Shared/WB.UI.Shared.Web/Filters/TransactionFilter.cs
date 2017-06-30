using System.Web.Mvc;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Transactions;

namespace WB.UI.Shared.Web.Filters
{
    public class TransactionFilter : ActionFilterAttribute
    {
        ITransactionManagerProvider TransactionManagerProvider => ServiceLocator.Current.GetInstance<ITransactionManagerProvider>();

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            this.TransactionManagerProvider.GetTransactionManager().BeginQueryTransaction();
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            var transactionManager = this.TransactionManagerProvider.GetTransactionManager();
            if (transactionManager.IsQueryTransactionStarted)
                transactionManager.RollbackQueryTransaction();
        }
    }
}