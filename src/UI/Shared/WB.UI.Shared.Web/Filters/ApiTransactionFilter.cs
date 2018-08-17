using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Shared.Web.Filters
{
    public class ApiTransactionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            var unitOfWork = ServiceLocator.Current.GetInstance<IUnitOfWork>();
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
