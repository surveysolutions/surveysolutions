using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Mvc;
using Ninject.Infrastructure.Language;
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
            if (!ShouldDisableTransaction(filterContext))
            {
                transactionManager.GetTransactionManager().BeginQueryTransaction();
            }
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            if (!ShouldDisableTransaction(filterContext))
            {
                transactionManager.GetTransactionManager().RollbackQueryTransaction();
            }
        }

        private static bool ShouldDisableTransaction(ActionExecutingContext actionContext)
        {
            return actionContext.ActionDescriptor.ControllerDescriptor.GetCustomAttributes(typeof(NoTransactionAttribute), inherit: true).Any();
        }

        private static bool ShouldDisableTransaction(ResultExecutedContext resultContext)
        {
            return resultContext.Controller.GetType().HasAttribute<NoTransactionAttribute>();
        }
    }
}