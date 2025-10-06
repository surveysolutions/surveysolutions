using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using WB.Enumerator.Native.WebInterview;
using WB.UI.Headquarters.API.WebInterview;
using WB.UI.Headquarters.Services;

namespace WB.UI.Headquarters.Filters
{
    public class WebInterviewResourcesAuthorizeAttribute : ActionFilterAttribute
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

            if (!HasAccessToWebInterview(context, interviewId, out var interviewAccessException) 
                && !HasAccessInReviewMode(context, interviewId, out interviewAccessException))
            {
                string errorMessage = WebInterview.GetUiMessageFromException(interviewAccessException);

                RedirectToError(context, errorMessage);
            }
        }

        private static bool HasAccessToWebInterview(ActionExecutingContext context, string interviewId, out InterviewAccessException interviewAccessException)
        {
            try
            {
                var services = context.HttpContext.RequestServices;
                services.GetRequiredService<IWebInterviewAllowService>().CheckWebInterviewAccessPermissions(interviewId);
                interviewAccessException = null;
                return true;
            }
            catch (InterviewAccessException e)
            {
                interviewAccessException = e;
                return false;
            }
        }

        private static bool HasAccessInReviewMode(ActionExecutingContext context, string interviewId, out InterviewAccessException interviewAccessException)
        {
            try
            {
                var services = context.HttpContext.RequestServices;
                services.GetRequiredService<IReviewAllowedService>().CheckIfAllowed(Guid.Parse(interviewId));
                interviewAccessException = null;
                return true;
            }
            catch (InterviewAccessException e)
            {
                interviewAccessException = e;
                return false;
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
