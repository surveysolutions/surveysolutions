using System.Web.Mvc;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Shared.Web.Filters
{
    public class TransactionFilter : ActionFilterAttribute
    {
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            //var unitOfWork = request.GetDependencyScope().GetService(typeof(IUnitOfWork)) as IUnitOfWork;

            var unitOfWork = ServiceLocator.Current.GetInstance<IUnitOfWork>();

            if (filterContext.Exception == null)
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
