using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Enumerator.Native.WebInterview;
using WB.UI.Headquarters.Models.WebInterview;
using AuthorizeAttribute = System.Web.Http.AuthorizeAttribute;

namespace WB.UI.Headquarters.API.WebInterview.Pipeline
{
    public class WebInterviewDataAuthorizeAttribute : AuthorizeAttribute
    {
        private const string InterviewIdQueryString = "interviewId";

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            try
            {
                CheckPermissions(actionContext);
            }
            catch (InterviewAccessException ie)
            {
                string errorMessage = WB.Enumerator.Native.WebInterview.WebInterview.GetUiMessageFromException(ie);

                actionContext.Response = new HttpResponseMessage(HttpStatusCode.NotAcceptable)
                {
                    Content = new ObjectContent<string>(errorMessage, new JsonMediaTypeFormatter())
                };
            }
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            try
            {
                CheckPermissions(actionContext);

                actionContext.Response = actionContext.RequestContext.Principal.Identity.IsAuthenticated
                    ? new HttpResponseMessage(HttpStatusCode.Forbidden) 
                    : new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
            catch (InterviewAccessException ie)
            {
                string errorMessage = WB.Enumerator.Native.WebInterview.WebInterview.GetUiMessageFromException(ie);

                actionContext.Response = new HttpResponseMessage(HttpStatusCode.NotAcceptable)
                {
                    Content = new ObjectContent<string>(errorMessage, new JsonMediaTypeFormatter())
                };
            }
        }

        private void CheckPermissions(HttpActionContext actionContext)
        {
            var queryString = actionContext.Request.RequestUri.ParseQueryString();
            var interviewId = queryString.Get(InterviewIdQueryString);
            var isReview = actionContext.Request.Headers.Contains(@"review");

            if (!isReview)
            {
                DependencyResolver.Current.GetService<IWebInterviewAllowService>().CheckWebInterviewAccessPermissions(interviewId);
            }
            else
            {
                DependencyResolver.Current.GetService<IReviewAllowedService>().CheckIfAllowed(Guid.Parse(interviewId));
            }
        }

    /*public override void OnAuthorization(AuthorizationContext filterContext)
    {
        base.OnAuthorization(filterContext);
    }

    protected override bool AuthorizeCore(HttpContextBase httpContext)
    {
        try
        {
            CheckPermissions(httpContext);
            return true;
        }
        catch (InterviewAccessException)
        {
            return false;
        }
    }

    protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
    {
        try
        {
            CheckPermissions(filterContext.HttpContext);

            filterContext.Result = filterContext.HttpContext.User.Identity.IsAuthenticated
                ? new HttpStatusCodeResult(403)
                : new HttpUnauthorizedResult();
        }
        catch (InterviewAccessException ie)
        {
            string errorMessage = WB.Enumerator.Native.WebInterview.WebInterview.GetUiMessageFromException(ie);

            filterContext.Result = new ViewResult
            {
                ViewName = @"~/Views/WebInterview/Error.cshtml",
                ViewData = new ViewDataDictionary(new WebInterviewError { Message = errorMessage })
            };
        }
    }


    //public override bool AuthorizeHubMethodInvocation(IHubIncomingInvokerContext hubIncomingInvokerContext,
        bool appliesToMethod)
    {
        return InScopeExecutor.Current.Execute((serviceLocatorLocal) =>
        {
            try
            {
                if (hubIncomingInvokerContext.MethodDescriptor.Attributes.Any(x =>
                    x.TypeId.ToString() == WebInterviewObserverNotAllowedAttribute.Id))
                {
                    if (serviceLocatorLocal.GetInstance<IAuthorizedUser>().IsObserving)
                        return false;
                }

                CheckPermissions(hubIncomingInvokerContext.Hub.Context.Request, serviceLocatorLocal);

                return true;
            }
            catch (InterviewAccessException)
            {
                var interviewId = hubIncomingInvokerContext.Hub.Context.QueryString.Get(@"interviewId");
                if (!interviewId.IsNullOrWhiteSpace())
                {
                    serviceLocatorLocal.GetInstance<IWebInterviewNotificationService>().ReloadInterview(Guid.Parse(interviewId));
                }
            }

            var authorizeHubMethodInvocation = base.AuthorizeHubMethodInvocation(hubIncomingInvokerContext, appliesToMethod);

            return authorizeHubMethodInvocation;
        });
    }

    private void CheckPermissions(HttpContextBase httpContext)
    {
        var interviewId = httpContext.Request.QueryString[InterviewIdQueryString];
        var isReview = httpContext.Request.Headers.Get(@"review")?.ToBool(false) ?? false;

        if (!isReview)
        {
            DependencyResolver.Current.GetService<IWebInterviewAllowService>().CheckWebInterviewAccessPermissions(interviewId);
        }
        else
        {
            DependencyResolver.Current.GetService<IReviewAllowedService>().CheckIfAllowed(Guid.Parse(interviewId));
        }
    }*/

}
}
