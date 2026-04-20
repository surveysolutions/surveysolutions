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
    /// Applied at controller level. Individual actions can opt-out via [SkipWebTesterSessionAuthorize].
    /// Resolves questionnaireId automatically from action arguments.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class WebTesterSessionAuthorizeAttribute : ActionFilterAttribute
    {
        private static readonly IReadOnlyList<string> WellKnownNames = ["questionnaireId", "id"];

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ActionDescriptor.EndpointMetadata
                    .OfType<SkipWebTesterSessionAuthorizeAttribute>().Any())
                return;

            var questionnaireId = ResolveQuestionnaireId(context.ActionArguments);
            if (questionnaireId == null)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
                return;
            }

            var sessionService = context.HttpContext.RequestServices
                .GetRequiredService<IWebTesterSessionService>();

            if (!sessionService.IsAuthorized(context.HttpContext.Session, questionnaireId.Value))
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
        }

        private static Guid? ResolveQuestionnaireId(IDictionary<string, object?> args)
        {
            foreach (var name in WellKnownNames)
            {
                if (args.TryGetValue(name, out var value) && value != null
                    && Guid.TryParse(value.ToString(), out var id))
                    return id;
            }

            foreach (var value in args.Values)
            {
                if (value != null && Guid.TryParse(value.ToString(), out var id))
                    return id;
            }

            return null;
        }
    }

    /// <summary>Exempts an action from WebTesterSessionAuthorize applied on the controller.</summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SkipWebTesterSessionAuthorizeAttribute : Attribute { }
}
