using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WB.Core.BoundedContexts.Headquarters.Services;
using Microsoft.Extensions.DependencyInjection;

namespace WB.UI.Headquarters.Filters
{
    public class ObserverNotAllowedAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var authorizedUser = filterContext.HttpContext.RequestServices.GetService<IAuthorizedUser>();

            if (authorizedUser.IsObserver)
                filterContext.Result = new ViewResult
                {
                    ViewName = "AccessDenied",
                    StatusCode = StatusCodes.Status403Forbidden
                };
            else
                base.OnActionExecuting(filterContext);
        }
    }
}
