using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using WB.Enumerator.Native.WebInterview;
using WB.UI.Headquarters.API.WebInterview;
using WB.UI.Headquarters.Models.WebInterview;

namespace WB.UI.Headquarters.Filters
{
    public class WebInterviewAuthorizeAttribute : ActionFilterAttribute
    {
        public string InterviewIdQueryString { get; set; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var interviewId = string.IsNullOrWhiteSpace(InterviewIdQueryString)
                ? context.RouteData.Values["id"].ToString()
                : context.HttpContext.Request.Query[InterviewIdQueryString].ToString();

            try
            {
                var webInterviewAllowService =
                    context.HttpContext.RequestServices.GetRequiredService<IWebInterviewAllowService>();
                webInterviewAllowService.CheckWebInterviewAccessPermissions(interviewId);
            }
            catch (InterviewAccessException ie)
            {
                string errorMessage = WebInterview.GetUiMessageFromException(ie);

                var controller = (Controller)context.Controller;

                context.Result = controller.View(@"~/Views/WebInterview/Error.cshtml",
                    new WebInterviewError {Message = errorMessage});
            }
        }
    }
}
