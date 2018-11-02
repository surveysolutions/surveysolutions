using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Main.Core.Entities.SubEntities;
using WB.UI.Headquarters.Resources;
using System.Threading.Tasks;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;

namespace WB.UI.Headquarters.Code
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiBasicAuthAttribute : AuthorizationFilterAttribute
    {
        public static string AuthHeader = @"WWW-Authenticate";
        
        public bool TreatPasswordAsPlain { get; set; } = false;
        public bool FallbackToCookieAuth { get; set; } = false;

        private readonly UserRoles[] roles;
        private readonly AuthorizeAttribute basicAuth;

        public ApiBasicAuthAttribute(params UserRoles[] roles)
        {
            this.roles = roles;
            this.basicAuth = new AuthorizeAttribute();
        }

        public override async Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            if (actionContext.Request.Headers?.Authorization == null)
            {
                if (FallbackToCookieAuth)
                {
                    await basicAuth.OnAuthorizationAsync(actionContext, cancellationToken);
                    appendBasicAuthHeader(actionContext);
                    return;
                }

                this.RespondWithMessageThatUserDoesNotExists(actionContext);
                return;
            }

            //resolve respecting curent scope
            var userManager = actionContext.Request.GetDependencyScope().GetService(typeof(HqSignInManager)) as HqSignInManager;

            var result = await userManager.SignInWithAuthTokenAsync(actionContext.Request.Headers?.Authorization?.ToString(), TreatPasswordAsPlain, this.roles);

            if (!result.Succeeded)
            {
                if (result.Errors.Any())
                {
                    if (result.Errors.Any(err => err.Contains(@"UpgradeRequired")))
                    {
                        RespondWithMessageThatUserRequireUpgrade(actionContext);
                        return;
                    }

                    if (result.Errors.Any(err => err.Contains(@"Role")))
                    {
                        RespondWithMessageThatUserIsNoPermittedRole(actionContext);
                        return;
                    }

                    if (result.Errors.Any(err => err.Contains(@"Lock")))
                    {
                        this.RespondWithMessageThatUserIsLockedOut(actionContext);
                        return;
                    }
                }
                else
                {
                    RespondWithMessageThatUserDoesNotExists(actionContext);
                    return;
                }
            }

            await base.OnAuthorizationAsync(actionContext, cancellationToken);
        }

        private void RespondWithMessageThatUserDoesNotExists(HttpActionContext actionContext)
        {
            actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                ReasonPhrase = string.Format(TabletSyncMessages.InvalidUserFormat, actionContext.Request.RequestUri.GetLeftPart(UriPartial.Authority))
            };
            appendBasicAuthHeader(actionContext);
        }

        private static void appendBasicAuthHeader(HttpActionContext actionContext)
        {
            actionContext.Response?.Headers.Add(AuthHeader, $@"Basic realm=""{actionContext.Request.RequestUri.DnsSafeHost}""");
        }

        private void RespondWithMessageThatUserIsNoPermittedRole(HttpActionContext actionContext)
        {
            actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { ReasonPhrase = TabletSyncMessages.InvalidUserRole };
            actionContext.Response.Headers.Add(AuthHeader, $@"Basic realm=""{actionContext.Request.RequestUri.DnsSafeHost}""");
        }

        private void RespondWithMessageThatUserRequireUpgrade(HttpActionContext actionContext)
        {
            actionContext.Response = new HttpResponseMessage(HttpStatusCode.UpgradeRequired) { ReasonPhrase = TabletSyncMessages.InterviewerApplicationShouldBeUpdated };
            actionContext.Response.Headers.Add(AuthHeader, $@"Basic realm=""{actionContext.Request.RequestUri.DnsSafeHost}""");
        }

        private void RespondWithMessageThatUserIsLockedOut(HttpActionContext actionContext)
        {
            actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized) {ReasonPhrase = @"lock"};
            actionContext.Response.Headers.Add(AuthHeader, $@"Basic realm=""{actionContext.Request.RequestUri.DnsSafeHost}""");
        }
    }
}
