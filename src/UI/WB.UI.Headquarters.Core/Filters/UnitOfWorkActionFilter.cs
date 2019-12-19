using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Headquarters.Filters
{
    public class UnitOfWorkActionFilter : IAsyncActionFilter
    {
        private readonly IUnitOfWork unitOfWork;

        public UnitOfWorkActionFilter(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var executedContext = await next.Invoke();
            if (executedContext.Exception == null)
            {
                await unitOfWork.AcceptChangesAsync();
            }
            else
            {
                unitOfWork.Dispose();
            }
        }
    }
}
