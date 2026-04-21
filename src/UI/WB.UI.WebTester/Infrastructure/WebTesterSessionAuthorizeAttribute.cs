using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester.Infrastructure
{
    /// <summary>
    /// Applied at controller level. Individual actions can opt-out via
    /// <see cref="SkipWebTesterSessionAuthorizeAttribute"/>.
    /// <para>
    /// Resolution strategy for <c>interviewId</c> (unique per run):
    /// <list type="number">
    ///   <item>Route arg named <c>"id"</c> — used by Interview / Status / Loading routes.</item>
    ///   <item>Route arg named <c>"questionnaireId"</c> → reverse session lookup
    ///         (<c>IWebTesterSessionService.GetInterviewId</c>) — used by ScenariosProxy.</item>
    /// </list>
    /// Once resolved and verified, sets <see cref="DesignerJwtContext.InterviewId"/> so that
    /// <see cref="DesignerJwtAuthHandler"/> can attach the correct delegated JWT to outbound calls.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class WebTesterSessionAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ActionDescriptor.EndpointMetadata
                    .OfType<SkipWebTesterSessionAuthorizeAttribute>().Any())
                return;

            var session = context.HttpContext.Session;
            var sessionService = context.HttpContext.RequestServices
                .GetRequiredService<IWebTesterSessionService>();

            Guid? interviewId = ResolveInterviewId(context.ActionArguments, session, sessionService);
            if (interviewId == null)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
                return;
            }

            if (!sessionService.IsAuthorized(session, interviewId.Value))
            {
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
                return;
            }

            // Make the resolved interviewId available to DesignerJwtAuthHandler
            // and UserContextMiddleware via AsyncLocal.
            DesignerJwtContext.InterviewId = interviewId;
        }

        private static Guid? ResolveInterviewId(
            IDictionary<string, object?> args,
            ISession session,
            IWebTesterSessionService sessionService)
        {
            // 1. "id" in the route is the per-run interviewId
            //    (Interview/{id}, Status/{id}, Loading/{id})
            if (args.TryGetValue("id", out var idValue)
                && idValue != null
                && Guid.TryParse(idValue.ToString(), out var interviewId))
                return interviewId;

            // 2. "questionnaireId" in the route → reverse session lookup
            //    (ScenariosProxy/{questionnaireId})
            if (args.TryGetValue("questionnaireId", out var qidValue)
                && qidValue != null
                && Guid.TryParse(qidValue.ToString(), out var questionnaireId))
                return sessionService.GetInterviewId(session, questionnaireId);

            // 3. Fallback: first Guid found in args (future-proofing)
            foreach (var value in args.Values)
            {
                if (value != null && Guid.TryParse(value.ToString(), out var fallback))
                    return fallback;
            }

            return null;
        }
    }

    /// <summary>Exempts an action from <see cref="WebTesterSessionAuthorizeAttribute"/> applied on the controller.</summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SkipWebTesterSessionAuthorizeAttribute : Attribute { }
}
