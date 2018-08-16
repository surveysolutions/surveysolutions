using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Shared.Web.Filters
{
    public class ApiTransactionFilter : ActionFilterAttribute
    {
        private readonly IUnitOfWork unitOfWork;

        public ApiTransactionFilter(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            this.unitOfWork.Start();
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Exception != null)
            {
                this.unitOfWork.AcceptChanges();
            }
            else
            {
                this.unitOfWork.Dispose();
            }
        }
    }
}
