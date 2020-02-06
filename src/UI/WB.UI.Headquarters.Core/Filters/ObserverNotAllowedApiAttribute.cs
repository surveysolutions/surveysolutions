using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using Microsoft.Extensions.DependencyInjection;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Filters
{
    public class ObserverNotAllowedApiAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var authorizedUser = filterContext.HttpContext.RequestServices.GetService<IAuthorizedUser>();
            
            if (authorizedUser.IsObserver)
            {
                filterContext.Result = new JsonResult(new { message = Strings.ObserverNotAllowed })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }
            else
                base.OnActionExecuting(filterContext);
        }
    }
}
