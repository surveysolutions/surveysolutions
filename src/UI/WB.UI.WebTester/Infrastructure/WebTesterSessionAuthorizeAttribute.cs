using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    /// Both the session flag (<c>auth:{interviewId}</c>) <b>and</b> the presence of a live
    /// delegated JWT in <see cref="IWebTesterJwtStore"/> must be valid.  If the JWT has expired
    /// the filter returns <c>401 Unauthorized</c> so the browser can re-trigger the code exchange
    /// rather than receiving a confusing <c>401</c> from an outbound Designer API call later.
    /// Once resolved and verified, sets <see cref="DesignerJwtContext.InterviewId"/> so that
    /// <see cref="DesignerJwtAuthHandler"/> can attach the correct delegated JWT to outbound calls.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class WebTesterSessionAuthorizeAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Validates the session and delegated JWT, sets
        /// <see cref="DesignerJwtContext.InterviewId"/> for the duration of the action, then
        /// <b>always</b> clears it in a <c>finally</c> block — even when the action throws or
        /// is short-circuited by an exception handler — so the AsyncLocal value can never leak
        /// into unrelated requests or background work that reuses the same ExecutionContext.
        /// </summary>
        public override async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            if (context.ActionDescriptor.EndpointMetadata
                    .OfType<SkipWebTesterSessionAuthorizeAttribute>().Any())
            {
                await next();
                return;
            }

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

            // Verify that the delegated JWT is still alive. If it has expired the session flag
            // is now stale — return 401 so the client knows it must start a fresh exchange rather
            // than silently sending requests that will fail with 401 on outbound Designer API calls.
            var jwtStore = context.HttpContext.RequestServices
                .GetRequiredService<IWebTesterJwtStore>();
            if (jwtStore.GetToken(interviewId.Value) == null)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized);
                return;
            }

            // Make the resolved interviewId available to DesignerJwtAuthHandler
            // and UserContextMiddleware via AsyncLocal for the duration of the action.
            // The finally block guarantees the value is cleared even when the action throws
            // or an exception filter short-circuits result execution, preventing cross-request
            // leakage into any subsequent work scheduled on the same ExecutionContext.
            DesignerJwtContext.InterviewId = interviewId;
            try
            {
                await next();
            }
            finally
            {
                DesignerJwtContext.InterviewId = null;
            }
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

