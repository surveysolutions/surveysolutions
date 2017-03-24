using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using WB.Core.Infrastructure.ReadSide;
using WB.UI.Headquarters.Resources;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;

namespace WB.UI.Headquarters.Code
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiBasicAuthAttribute : AuthorizationFilterAttribute
    {
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

            var result = await userManager.SignInWithAuthTokenAsync(actionContext.Request.Headers.Authorization.ToString(), TreatPasswordAsPlain, this.roles);

            if (!result.Succeeded)
            {
                if (result.Errors.Any())
                {
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

        private bool CheckHashedPassword(HqUser userInfo, BasicCredentials basicCredentials)
        {
            var compatibilityProvider = ServiceLocator.Current.GetInstance<IHashCompatibilityProvider>();

            if (compatibilityProvider.IsInSha1CompatibilityMode() && userInfo.IsInRole(UserRoles.Interviewer))
            {
                return userInfo.PasswordHashSha1 == basicCredentials.Password;
            }

            return userInfo.PasswordHash == basicCredentials.Password;
        }

        private BasicCredentials ExtractFromAuthorizationHeader(ApiAuthenticationScheme scheme, HttpActionContext actionContext)
        {
            try
            {
                string schemeString = scheme.ToString();
                string header = actionContext?.Request.Headers?.Authorization?.ToString();

                if (header == null) return null;
                if (!header.StartsWith(schemeString, StringComparison.OrdinalIgnoreCase)) return null;

                string credentials = Encoding.ASCII.GetString(Convert.FromBase64String(header.Substring(schemeString.Length + 1)));
                int splitOn = credentials.IndexOf(':');

                return new BasicCredentials
                {
                    Username = credentials.Substring(0, splitOn),
                    Password = credentials.Substring(splitOn + 1),
                    Scheme = scheme
                };
            }
            catch
            {
            }

            return null;
        }

        internal class BasicCredentials
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public ApiAuthenticationScheme Scheme { get; set; }
        }

        private void RespondWithMessageThatUserDoesNotExists(HttpActionContext actionContext)
        {
            actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { ReasonPhrase = string.Format(TabletSyncMessages.InvalidUserFormat, actionContext.Request.RequestUri.GetLeftPart(UriPartial.Authority)) };
            actionContext.Response.Headers.Add("WWW-Authenticate", string.Format("Basic realm=\"{0}\"", actionContext.Request.RequestUri.DnsSafeHost));
        }

        private void RespondWithMaintenanceMessage(HttpActionContext actionContext)
        {
            actionContext.Response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) { ReasonPhrase = TabletSyncMessages.Maintenance };
        }

        private void RespondWithMessageThatUserIsNoPermittedRole(HttpActionContext actionContext)
        {
            actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { ReasonPhrase = TabletSyncMessages.InvalidUserRole };
        }
        private void RespondWithMessageThatUserIsLockedOut(HttpActionContext actionContext)
            => actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized) {ReasonPhrase = @"lock"};
    }
}