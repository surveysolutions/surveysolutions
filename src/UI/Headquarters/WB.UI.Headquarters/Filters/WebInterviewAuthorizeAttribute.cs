using System.Web;
using System.Web.Mvc;
using WB.Enumerator.Native.WebInterview;
using WB.UI.Headquarters.API.WebInterview;
using WB.UI.Headquarters.Models.WebInterview;

namespace WB.UI.Headquarters.Filters
{
    public class WebInterviewAuthorizeAttribute : AuthorizeAttribute
    {
        public WebInterviewAuthorizeAttribute()
        {
            this.Order = 30;
        }

        private IWebInterviewAllowService webInterviewAllowService => DependencyResolver.Current.GetService<IWebInterviewAllowService>();

        public string InterviewIdQueryString { get; set; }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var interviewId = string.IsNullOrWhiteSpace(InterviewIdQueryString) 
                ? httpContext.Request.RequestContext.RouteData.Values["id"].ToString() 
                : httpContext.Request.QueryString[InterviewIdQueryString];

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
            var interviewId = string.IsNullOrWhiteSpace(InterviewIdQueryString)
                ? filterContext.HttpContext.Request.RequestContext.RouteData.Values["id"].ToString()
                : filterContext.HttpContext.Request.QueryString[InterviewIdQueryString];

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
