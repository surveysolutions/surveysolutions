using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Enumerator.Native.WebInterview;

namespace WB.UI.Headquarters.Filters
{
    public class WebInterviewObserverNotAllowedActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            IAuthorizedUser authorizedUser = (IAuthorizedUser) actionContext.Request.GetDependencyScope().GetService(typeof(IAuthorizedUser));

            if (actionContext.ActionDescriptor.GetCustomAttributes<WebInterviewObserverNotAllowedAttribute>().Count > 0 
                && authorizedUser.IsObserver)
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Forbidden);
            }
        }
    }
}
