using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.DataCollection.Aggregates;
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

            // Check review mode first: only for authenticated users who can conduct interview reviews.
            // This avoids an unnecessary DB lookup for anonymous respondents.
            var authorizedUser = context.HttpContext.RequestServices.GetRequiredService<IAuthorizedUser>();
            if (authorizedUser.CanConductInterviewReview() && HasAccessInReviewMode(context, interviewId, out _))
                return;

            // Fall back to web interview access + password verification for regular respondents
            if (!HasAccessToWebInterview(context, interviewId, out var interviewAccessException))
            {
                string errorMessage = WebInterview.GetUiMessageFromException(interviewAccessException);
                RedirectToError(context, errorMessage);
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
            if (!Guid.TryParse(interviewId, out var interviewGuid))
            {
                interviewAccessException = null;
                return false;
            }

            try
            {
                var services = context.HttpContext.RequestServices;
                services.GetRequiredService<IReviewAllowedService>().CheckIfAllowed(interviewGuid);
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
            // Short-circuit: if the session already indicates password was verified, skip DB lookups
            if (context.HttpContext.Session.IsPasswordVerifiedForInterview(interviewId))
                return true;

            // Prototype interviews have no assignment password — access was already granted by the
            // allow-service's prototype fast-path before loading any interview entity.
            if (context.HttpContext.Items.ContainsKey(IWebInterviewAllowService.PrototypeAccessGrantedKey))
                return true;

            // Reuse the interview already loaded (and cached) by HasAccessToWebInterview to avoid
            // an extra repository round-trip per resource request.
            context.HttpContext.Items.TryGetValue(IWebInterviewAllowService.CachedInterviewItemsKey, out var cachedItem);
            var interview = cachedItem as IStatefulInterview;

            if (interview == null)
            {
                var services = context.HttpContext.RequestServices;
                var interviewRepository = services.GetRequiredService<IStatefulInterviewRepository>();
                interview = interviewRepository.Get(interviewId);
            }

            // Fail-closed: if the interview cannot be loaded here, deny access
            if (interview == null) return false;

            var assignmentId = interview.GetAssignmentId();
            // Interviews without an assignment have no assignment password to check
            if (!assignmentId.HasValue) return true;

            var assignmentsService = context.HttpContext.RequestServices.GetRequiredService<IAssignmentsService>();
            var assignment = assignmentsService.GetAssignment(assignmentId.Value);

            // If no password is set on the assignment, access is not password-restricted
            if (string.IsNullOrWhiteSpace(assignment?.Password)) return true;

            return false;
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
