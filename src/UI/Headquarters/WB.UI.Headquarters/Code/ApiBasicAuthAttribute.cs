using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Main.Core.Entities.SubEntities;
using WB.Core.Infrastructure.ReadSide;
using WB.UI.Headquarters.Resources;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.UI.Headquarters.Code
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiBasicAuthAttribute : AuthorizationFilterAttribute
    {
        public static string AuthHeader = @"WWW-Authenticate";

        private HqSignInManager userManager => ServiceLocator.Current.GetInstance<HqSignInManager>();

        private IReadSideStatusService readSideStatusService
            => ServiceLocator.Current.GetInstance<IReadSideStatusService>();

        public bool TreatPasswordAsPlain { get; set; } = false;
        private readonly UserRoles[] roles;

        public ApiBasicAuthAttribute(params UserRoles[] roles)
        {
            this.roles = roles;
        }

        public override async Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            if (this.readSideStatusService.AreViewsBeingRebuiltNow())
            {
                this.RespondWithMaintenanceMessage(actionContext);
                return;
            }

            if (actionContext.Request.Headers?.Authorization == null)
            {
                this.RespondWithMessageThatUserDoesNotExists(actionContext);
                return;
            }

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
            actionContext.Response.Headers.Add(AuthHeader, $@"Basic realm=""{actionContext.Request.RequestUri.DnsSafeHost}""");
        }

        private void RespondWithMaintenanceMessage(HttpActionContext actionContext)
        {
            actionContext.Response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) { ReasonPhrase = TabletSyncMessages.Maintenance };
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