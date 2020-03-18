using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.UI.Shared.Web.Attributes;

namespace WB.UI.Headquarters.Filters
{
    public class UnitOfWorkActionFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var executedContext = await next.Invoke();

            var doesMarkAsNoTransaction = DoesMarkAsNoTransaction(context);
            if (doesMarkAsNoTransaction)
                return;

            var unitOfWork = context.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>();
            if (executedContext.Exception == null)
            {
                unitOfWork.AcceptChanges();
            }
            else
            {
                unitOfWork.Dispose();
            }
        }

        private bool DoesMarkAsNoTransaction(ActionExecutingContext context)
        {
            if (context.Filters.OfType<NoTransactionAttribute>().Any())
                return true;

            return false;
        }
    }
}
