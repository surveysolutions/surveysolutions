using System.Linq;
using System.Web.Mvc;
using Ninject.Infrastructure.Language;
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
            if (!ShouldDisableTransaction(filterContext))
            {
                transactionManager.BeginTransaction();
            }
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            if (!ShouldDisableTransaction(filterContext))
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