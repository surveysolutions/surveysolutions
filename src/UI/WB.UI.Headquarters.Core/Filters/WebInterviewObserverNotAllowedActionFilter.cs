using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Enumerator.Native.WebInterview;

namespace WB.UI.Headquarters.Filters
{
    public class WebInterviewObserverNotAllowedActionFilter : IActionFilter
    {
        private readonly IAuthorizedUser authorizedUser;

        public WebInterviewObserverNotAllowedActionFilter(IAuthorizedUser authorizedUser)
        {
            this.authorizedUser = authorizedUser;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            
        }

        public void OnActionExecuting(ActionExecutingContext actionContext)
        {
            if (actionContext.ActionDescriptor is ControllerActionDescriptor controllerContext &&
                controllerContext.MethodInfo.GetCustomAttributes(true).OfType<WebInterviewObserverNotAllowedAttribute>()
                    .Any() && authorizedUser.IsObserver)
            {
                actionContext.Result = new ForbidResult();
            }
        }
    }
}
