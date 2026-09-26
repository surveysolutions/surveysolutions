using System.Linq;
using System.Threading.Tasks;
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
            if (DoesMarkAsNoTransaction(context))
            {
                await next.Invoke();
                return;
            }

            var unitOfWork = context.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>();
            ActionExecutedContext executedContext;
            try
            {
                executedContext = await next.Invoke();
            }
            catch
            {
                unitOfWork.DiscardChanges();
                throw;
            }

            if (executedContext.Exception == null)
            {
                unitOfWork.AcceptChanges();
                // Action filters finish before MVC executes/serializes the result.
                unitOfWork.Complete();
            }
            else
            {
                // The request scope owns disposal. Other filters may still need its services.
                unitOfWork.DiscardChanges();
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
