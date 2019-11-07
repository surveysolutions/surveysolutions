using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Shared.Web.Filters
{
    public class ApiTransactionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            //should respect current execution scope
            //but filter is a singletone
            var unitOfWork = context.Request.GetDependencyScope().GetService(typeof(IUnitOfWork)) as IUnitOfWork;

            if (context.Exception == null)
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
