using System.Web.Mvc;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Shared.Web.Filters
{
    public class TransactionFilter : ActionFilterAttribute
    {
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            //should respect current scope
            var unitOfWork = DependencyResolver.Current.GetService<IUnitOfWork>() as IUnitOfWork;
            
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
