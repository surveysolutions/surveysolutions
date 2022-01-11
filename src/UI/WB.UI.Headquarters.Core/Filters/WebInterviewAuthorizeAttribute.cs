using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using WB.Enumerator.Native.WebInterview;
using WB.UI.Headquarters.API.WebInterview;
using WB.UI.Headquarters.Models.WebInterview;
using WB.UI.Headquarters.Services;

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

            if (string.IsNullOrEmpty(interviewId))
            {
                RedirectToError(context, Enumerator.Native.Resources.WebInterview.InterviewNotFound);
                return;
            }

            try
            {
                var ctx = context.HttpContext;
                var isReview = ctx.Request.Headers.ContainsKey(@"review");
                var services = context.HttpContext.RequestServices;

                if (isReview)
                {
                    services.GetRequiredService<IReviewAllowedService>().CheckIfAllowed(Guid.Parse(interviewId));
                }
                else
                {
                    services.GetRequiredService<IWebInterviewAllowService>().CheckWebInterviewAccessPermissions(interviewId);
                }
            }
            catch (InterviewAccessException ie)
            {
                string errorMessage = WebInterview.GetUiMessageFromException(ie);

                RedirectToError(context, errorMessage);
            }
        }

        private void RedirectToError(ActionExecutingContext context, string errorMessage)
        {
            if (context.Controller is Controller controller)
            {
                controller.TempData["WebInterview.ErrorMessage"] = errorMessage;
            }

            context.Result = new RedirectToActionResult("Error", "WebInterview", null); 
        }
    }
}
