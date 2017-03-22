using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Security;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.Core.BoundedContexts.Designer.Views.AllowedAddresses;
using WB.Core.Infrastructure.Transactions;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class ApiBasicAuthAttribute : AuthorizationFilterAttribute
    {
        private readonly bool onlyAllowedAddresses;
        private IMembershipUserService UserHelper => ServiceLocator.Current.GetInstance<IMembershipUserService>();

        private IAllowedAddressService allowedAddressService => ServiceLocator.Current.GetInstance<IAllowedAddressService>();

        private IPlainTransactionManagerProvider TransactionManagerProvider => ServiceLocator.Current.GetInstance<IPlainTransactionManagerProvider>();

        private readonly Func<string, string, bool> validateUserCredentials;

        public ApiBasicAuthAttribute(bool onlyAllowedAddresses = false)
            : this(Membership.ValidateUser)
        {
            this.onlyAllowedAddresses = onlyAllowedAddresses;
        }

        internal ApiBasicAuthAttribute(Func<string, string, bool> validateUserCredentials)
        {
            this.validateUserCredentials = validateUserCredentials;
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var credentials = ParseCredentials(actionContext);
            if (credentials == null)
            {
                this.ThrowUnathorizedException(actionContext, ErrorMessages.User_Not_authorized);
                return;
            }
            this.TransactionManagerProvider.GetPlainTransactionManager().BeginTransaction();
            try
            {
                if (!this.Authorize(credentials.Username, credentials.Password))
                {
                    this.ThrowUnathorizedException(actionContext, ErrorMessages.User_Not_authorized);
                    return;
                }

                var identity = new GenericIdentity(credentials.Username, "Basic");
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
                    var ip = GetClientIpAddress();
                    if (!allowedAddressService.IsAllowedAddress(ip) && !this.UserHelper.WebUser.CanImportOnHq)
                    {
                        this.ThrowUnathorizedException(actionContext, ErrorMessages.UserNeedToContactSupport);
                        return;
                    }
                }


                base.OnAuthorization(actionContext);
            }
            finally
            {
                this.TransactionManagerProvider.GetPlainTransactionManager().RollbackTransaction();
            }
        }

        private IPAddress GetClientIpAddress()
        {
            IPAddress ip = null;
            var userHostAddress = HttpContext.Current.Request.UserHostAddress;

            IPAddress.TryParse(userHostAddress, out ip);

            var xForwardedForKey = HttpContext.Current.Request.ServerVariables.AllKeys.FirstOrDefault(x => x.ToUpper() == "X_FORWARDED_FOR");

            if (string.IsNullOrEmpty(xForwardedForKey))
                return ip;

            var xForwardedFor = HttpContext.Current.Request.ServerVariables[xForwardedForKey];

            if (string.IsNullOrEmpty(xForwardedFor))
                return ip;

            return xForwardedFor.Split(',').Select(IPAddress.Parse).LastOrDefault(x => !IsPrivateIpAddress(x));
        }

        private static bool IsPrivateIpAddress(IPAddress ip)
        {
            // http://en.wikipedia.org/wiki/Private_network
            // Private IP Addresses are: 
            //  24-bit block: 10.0.0.0 through 10.255.255.255
            //  20-bit block: 172.16.0.0 through 172.31.255.255
            //  16-bit block: 192.168.0.0 through 192.168.255.255
            //  Link-local addresses: 169.254.0.0 through 169.254.255.255 (http://en.wikipedia.org/wiki/Link-local_address)

            var octets = ip.GetAddressBytes();

            var is24BitBlock = octets[0] == 10;
            if (is24BitBlock) return true; // Return to prevent further processing

            var is20BitBlock = octets[0] == 172 && octets[1] >= 16 && octets[1] <= 31;
            if (is20BitBlock) return true; // Return to prevent further processing

            var is16BitBlock = octets[0] == 192 && octets[1] == 168;
            if (is16BitBlock) return true; // Return to prevent further processing

            var isLinkLocalAddress = octets[0] == 169 && octets[1] == 254;
            return isLinkLocalAddress;
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
            actionContext.Response.Headers.Add("WWW-Authenticate", string.Format("Basic realm=\"{0}\"", host));
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
                    string.Format(ErrorMessages.UserNotApproved, this.UserHelper.WebUser.MembershipUser.Email)
            };
        }

        private bool Authorize(string username, string password)
        {
            return validateUserCredentials(username, password);
        }

        private bool IsAccountLockedOut()
        {
            return this.UserHelper.WebUser == null || this.UserHelper.WebUser.MembershipUser.IsLockedOut;
        }

        private bool IsAccountNotApproved()
        {
            return !this.UserHelper.WebUser.MembershipUser.IsApproved;
        }
    }
}