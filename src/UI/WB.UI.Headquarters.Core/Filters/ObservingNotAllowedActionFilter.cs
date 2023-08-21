using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Enumerator.Native.WebInterview;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Filters
{
    public class ObservingNotAllowedActionFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            
        }

        public void OnActionExecuting(ActionExecutingContext actionContext)
        {
            if (actionContext.ActionDescriptor is ControllerActionDescriptor controllerContext &&
                (controllerContext.MethodInfo.GetCustomAttributes(true).OfType<ObservingNotAllowedAttribute>().Any() ||
                controllerContext.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<ObservingNotAllowedAttribute>().Any()))
            {
                IAuthorizedUser authorizedUser = actionContext.HttpContext.RequestServices.GetRequiredService<IAuthorizedUser>();
                if (authorizedUser.IsObserving)
                {
                    actionContext.Result = actionContext.HttpContext.Request.IsJsonRequest()
                        ? (IActionResult)new JsonResult(new { message = Strings.ObserverNotAllowed })
                        {
                            StatusCode = StatusCodes.Status403Forbidden
                        }
                        : new ViewResult
                        {
                            ViewName = "AccessDenied",
                            StatusCode = StatusCodes.Status403Forbidden
                        };
                }
            }
        }
    }
}
