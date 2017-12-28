using System;
using System.Web;
using System.Web.Mvc;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Models;
using WB.UI.Headquarters.API.WebInterview;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Filters
{
    public class WebInterviewAuthorizeAttribute : AuthorizeAttribute
    {
        public WebInterviewAuthorizeAttribute()
        {
            this.Order = 30;
        }

        private IWebInterviewAllowService webInterviewAllowService => ServiceLocator.Current.GetInstance<IWebInterviewAllowService>();

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var interviewId = httpContext.Request.RequestContext.RouteData.Values["id"].ToString();
            try
            {
                webInterviewAllowService.CheckWebInterviewAccessPermissions(interviewId);
                return true;
            }
            catch (InterviewAccessException)
            {
                return false;
            }
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var interviewId = filterContext.RequestContext.RouteData.Values["id"].ToString();
            try
            {
                webInterviewAllowService.CheckWebInterviewAccessPermissions(interviewId);

                filterContext.Result = filterContext.HttpContext.User.Identity.IsAuthenticated 
                    ? new HttpStatusCodeResult(403) 
                    : new HttpUnauthorizedResult();
            }
            catch (InterviewAccessException ie)
            {
                string errorMessage = WebInterview.GetUiMessageFromException(ie);

                filterContext.Result = new ViewResult
                {
                    ViewName = @"~/Views/WebInterview/Error.cshtml",
                    ViewData = new ViewDataDictionary(new WebInterviewError { Message = errorMessage })
                };
            }
        }
    }
}