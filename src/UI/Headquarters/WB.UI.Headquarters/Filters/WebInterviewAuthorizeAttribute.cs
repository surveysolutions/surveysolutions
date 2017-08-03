using System.Web.Mvc;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.UI.Headquarters.API.WebInterview;

namespace WB.UI.Headquarters.Filters
{
    public class WebInterviewAuthorizeAttribute : ActionFilterAttribute
    {
        public WebInterviewAuthorizeAttribute()
        {
            this.Order = 30;
        }

        private IWebInterviewAllowService webInterviewAllowService => ServiceLocator.Current.GetInstance<IWebInterviewAllowService>();

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var interviewId = filterContext.ActionParameters[@"id"].ToString();
            webInterviewAllowService.CheckWebInterviewAccessPermissions(interviewId);
        }
    }
}