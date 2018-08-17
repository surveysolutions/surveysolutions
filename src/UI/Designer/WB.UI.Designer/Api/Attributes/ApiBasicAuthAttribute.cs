using System;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Security;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Services.Accounts;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Transactions;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class ApiBasicAuthAttribute : AuthorizationFilterAttribute
    {
        private readonly bool onlyAllowedAddresses;
        private IMembershipUserService MembershipService => ServiceLocator.Current.GetInstance<IMembershipUserService>();

        private IAllowedAddressService allowedAddressService => ServiceLocator.Current.GetInstance<IAllowedAddressService>();

        private IIpAddressProvider ipAddressProvider => ServiceLocator.Current.GetInstance<IIpAddressProvider>();

        private IAccountRepository AccountRepository => ServiceLocator.Current.GetInstance<IAccountRepository>();

        private readonly Func<string, string, bool> validateUserCredentials;

        public ApiBasicAuthAttribute(bool onlyAllowedAddresses = false)
            : this(Membership.ValidateUser, onlyAllowedAddresses)
        {
        }

        internal ApiBasicAuthAttribute(Func<string, string, bool> validateUserCredentials, bool onlyAllowedAddresses = false)
        {
            this.validateUserCredentials = validateUserCredentials;
            this.onlyAllowedAddresses = onlyAllowedAddresses;
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var credentials = ParseCredentials(actionContext);
            if (credentials == null)
            {
                this.ThrowUnathorizedException(actionContext, ErrorMessages.User_Not_authorized);
                return;
            }

            if (!this.Authorize(credentials.Username, credentials.Password))
            {
                this.ThrowUnathorizedException(actionContext, ErrorMessages.User_Not_authorized);
                return;
            }

            var account = this.AccountRepository.GetByNameOrEmail(credentials.Username);
            var identity = new GenericIdentity(account.UserName, "Basic");
            var principal = new GenericPrincipal(identity, null);

            Thread.CurrentPrincipal = principal;
            if (HttpContext.Current != null)
            {
                HttpContext.Current.User = principal;
            }

            if (IsAccountLockedOut())
            {
                this.ThrowLockedOutException(actionContext);
                return;
            }

            if (this.IsAccountNotApproved())
            {
                this.ThrowNotApprovedException(actionContext);
                return;
            }

            if (this.onlyAllowedAddresses)
            {
                if (!this.MembershipService.WebUser.MembershipUser.CanImportOnHq)
                {
                    var clientIpAddress = ipAddressProvider.GetClientIpAddress();
                    if (!this.allowedAddressService.IsAllowedAddress(clientIpAddress))
                    {
                        this.ThrowForbiddenException(actionContext, string.Format(ErrorMessages.UserNeedToContactSupportFormat, clientIpAddress));
                        return;
                    }
                }
            }

            base.OnAuthorization(actionContext);
        }

        private static BasicCredentials ParseCredentials(HttpActionContext actionContext)
        {
            try
            {
                if (actionContext.Request.Headers.Authorization != null && actionContext.Request.Headers.Authorization.Scheme == "Basic")
                {
                    var credentials =
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
            catch {}

            return null;
        }

        internal class BasicCredentials
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        private void ThrowUnathorizedException(HttpActionContext actionContext, string errorMessage)
        {
            var host = actionContext.Request.RequestUri.DnsSafeHost;
            actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { ReasonPhrase = errorMessage };
            actionContext.Response.Headers.Add("WWW-Authenticate", $"Basic realm=\"{host}\"");
        }

        private void ThrowForbiddenException(HttpActionContext actionContext, string errorMessage)
        {
            var host = actionContext.Request.RequestUri.DnsSafeHost;
            actionContext.Response = new HttpResponseMessage(HttpStatusCode.Forbidden) { ReasonPhrase = errorMessage };
            actionContext.Response.Headers.Add("WWW-Authenticate", $"Basic realm=\"{host}\"");
        }

        private void ThrowLockedOutException(HttpActionContext actionContext)
        {
            actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { ReasonPhrase = ErrorMessages.UserLockedOut };
        }

        private void ThrowNotApprovedException(HttpActionContext actionContext)
        {
            actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                ReasonPhrase =
                    string.Format(ErrorMessages.UserNotApproved, this.MembershipService.WebUser.MembershipUser.Email)
            };
        }

        private bool Authorize(string username, string password)
        {
            return validateUserCredentials(username, password);
        }

        private bool IsAccountLockedOut()
        {
            return this.MembershipService.WebUser == null || this.MembershipService.WebUser.MembershipUser.IsLockedOut;
        }

        private bool IsAccountNotApproved()
        {
            return !this.MembershipService.WebUser.MembershipUser.IsApproved;
        }
    }
}
