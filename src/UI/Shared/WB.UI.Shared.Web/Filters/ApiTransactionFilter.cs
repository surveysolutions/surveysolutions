using System.Net.Http;
using System.Web.Http.Filters;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Shared.Web.Filters
{
    public class ApiTransactionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            //should respect current execution scope
            //but filter is a singletone
            var unitOfWork = actionExecutedContext.Request.GetDependencyScope().GetService(typeof(IUnitOfWork)) as IUnitOfWork;
            
            if (actionExecutedContext.Exception == null)
            {
                unitOfWork.AcceptChanges();
            }
            else
            {
                unitOfWork.Dispose();
            }
        }
    }
}
