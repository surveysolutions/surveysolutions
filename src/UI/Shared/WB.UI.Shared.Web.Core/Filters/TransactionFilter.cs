using System.Threading;
using System.Web.Mvc;
using Autofac;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.UI.Shared.Web.Kernel;

namespace WB.UI.Shared.Web.Filters
{
    public class TransactionFilter : ActionFilterAttribute
    {
        private readonly IUnitOfWork unitOfWork;

        public TransactionFilter(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            base.OnResultExecuted(filterContext);
            
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
