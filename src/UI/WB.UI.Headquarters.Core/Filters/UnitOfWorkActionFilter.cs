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
            var doesMarkAsNoTransaction = DoesMarkAsNoTransaction(context);
            if (doesMarkAsNoTransaction)
                return;

            var executedContext = await next.Invoke();

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
            if (context.Controller.GetType().GetCustomAttributes(typeof(NoTransactionAttribute), true).Length > 0)
                return true;

            if (context.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
            {
                // Check if the attribute exists on the action method
                if (controllerActionDescriptor.MethodInfo?.GetCustomAttributes(inherit: true)?.Any(a => a.GetType() == typeof(NoTransactionAttribute)) ?? false)
                    return true;

                // Check if the attribute exists on the controller
                if (controllerActionDescriptor.ControllerTypeInfo?.GetCustomAttributes(typeof(NoTransactionAttribute), true)?.Any() ?? false)
                    return true;
            }

            return false;
        }
    }
}
