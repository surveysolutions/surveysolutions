using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.WebInterview;
using WB.UI.Headquarters.API.WebInterview;
using WB.UI.Headquarters.Code.WebInterview;
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

            bool hasWebAccess = HasAccessToWebInterview(context, interviewId, out var interviewAccessException);

            if (!hasWebAccess)
            {
                if (!HasAccessInReviewMode(context, interviewId, out _))
                {
                    string errorMessage = WebInterview.GetUiMessageFromException(interviewAccessException);
                    RedirectToError(context, errorMessage);
                }
                // Access via review mode - no password check needed
                return;
            }

            // Access via web interview - check if password has been verified
            if (!IsInterviewPasswordVerified(context, interviewId))
            {
                RedirectToError(context, Enumerator.Native.Resources.WebInterview.Error_UserNotAuthorised);
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

        private static bool IsInterviewPasswordVerified(ActionExecutingContext context, string interviewId)
        {
            var services = context.HttpContext.RequestServices;
            var interviewRepository = services.GetRequiredService<IStatefulInterviewRepository>();
            var interview = interviewRepository.Get(interviewId);

            // Fail-closed: if the interview cannot be loaded here, deny access
            if (interview == null) return false;

            var assignmentId = interview.GetAssignmentId();
            // Interviews without an assignment have no assignment password to check
            if (!assignmentId.HasValue) return true;

            var assignmentsService = services.GetRequiredService<IAssignmentsService>();
            var assignment = assignmentsService.GetAssignment(assignmentId.Value);

            // If no password is set on the assignment, access is not password-restricted
            if (string.IsNullOrWhiteSpace(assignment?.Password)) return true;

            return context.HttpContext.Session.IsPasswordVerifiedForInterview(interviewId);
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
