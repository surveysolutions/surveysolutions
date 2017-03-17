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
using Microsoft.AspNet.Identity;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.Infrastructure.ReadSide;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Code
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiBasicAuthAttribute : AuthorizationFilterAttribute
    {
        private HqUserManager userManager => ServiceLocator.Current.GetInstance<HqUserManager>();

        private IReadSideStatusService readSideStatusService
            => ServiceLocator.Current.GetInstance<IReadSideStatusService>();

        public bool TreatPasswordAsPlain { get; set; } = false;
        private readonly UserRoles[] roles;

        public ApiBasicAuthAttribute(UserRoles[] roles)
        {
            this.roles = roles;
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (this.readSideStatusService.AreViewsBeingRebuiltNow())
            {
                this.RespondWithMaintenanceMessage(actionContext);
                return;
            }

            BasicCredentials basicCredentials = ParseCredentials(actionContext);
            if(basicCredentials == null)
            {
                this.RespondWithMessageThatUserDoesNotExists(actionContext);
                return;
            }

            var userInfo = this.userManager.FindByName(basicCredentials.Username);
            if (userInfo == null || userInfo.IsArchived)
            {
                this.RespondWithMessageThatUserDoesNotExists(actionContext);
                return;
            }

            if (userInfo.IsLockedBySupervisor || userInfo.IsLockedByHeadquaters)
            {
                this.RespondWithMessageThatUserIsLockedOut(actionContext);
                return;
            }

            if ((this.TreatPasswordAsPlain && !this.userManager.CheckPassword(userInfo, basicCredentials.Password)) ||
                (!this.TreatPasswordAsPlain && (userInfo.PasswordHash != basicCredentials.Password)))
            {
                this.RespondWithMessageThatUserDoesNotExists(actionContext);
                return;
            }

            if (!this.roles.Contains(userInfo.Roles.First().Role))
            {
                this.RespondWithMessageThatUserIsNoPermittedRole(actionContext);
                return;
            }

            var identity = userInfo.GenerateUserIdentityAsync(this.userManager).Result;
            var principal = new ClaimsPrincipal(identity);

            Thread.CurrentPrincipal = principal;
            if (HttpContext.Current != null)
            {
                HttpContext.Current.User = principal;
            }

            base.OnAuthorization(actionContext);
        }


        private static BasicCredentials ParseCredentials(HttpActionContext actionContext)
        {
            try
            {
                if (actionContext.Request.Headers.Authorization != null &&
                    actionContext.Request.Headers.Authorization.Scheme == "Basic")
                {
                    string credentials =
                        Encoding.ASCII.GetString(
                            Convert.FromBase64String(actionContext.Request.Headers.Authorization.ToString().Substring(6)));
                    int splitOn = credentials.IndexOf(':');

                    return new BasicCredentials
                    {
                        Username = credentials.Substring(0, splitOn),
                        Password = credentials.Substring(splitOn + 1)
                    };
                }
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