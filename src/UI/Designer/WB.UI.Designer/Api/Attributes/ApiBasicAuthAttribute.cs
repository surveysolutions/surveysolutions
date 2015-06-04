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
using Microsoft.Practices.ServiceLocation;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.Transactions;
using WB.UI.Designer.Resources;
using WB.UI.Shared.Web.Membership;

namespace WB.UI.Designer.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class ApiBasicAuthAttribute : AuthorizationFilterAttribute
    {
        private IMembershipUserService userHelper
        {
            get { return ServiceLocator.Current.GetInstance<IMembershipUserService>(); }
        }

        private IReadSideStatusService readSideStatusService
        {
            get { return ServiceLocator.Current.GetInstance<IReadSideStatusService>(); }
        }

        private ITransactionManagerProvider TransactionManagerProvider
        {
            get { return ServiceLocator.Current.GetInstance<ITransactionManagerProvider>(); }
        }

        private readonly Func<string, string, bool> validateUserCredentials;

        public ApiBasicAuthAttribute()
            : this(Membership.ValidateUser){}

        internal ApiBasicAuthAttribute(Func<string, string, bool> validateUserCredentials)
        {
            this.validateUserCredentials = validateUserCredentials;
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (this.readSideStatusService.AreViewsBeingRebuiltNow())
            {
                this.ResponseMaintenanceMessage(actionContext);
                return;
            }

            var credentials = ParseCredentials(actionContext);
            if (credentials == null)
            {
                this.ThrowUnathorizedException(actionContext);
                return;
            }
            this.TransactionManagerProvider.GetTransactionManager().BeginQueryTransaction();
            try
            {
                if (!this.Authorize(credentials.Username, credentials.Password))
                {
                    this.ThrowUnathorizedException(actionContext);
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

                base.OnAuthorization(actionContext);
            }
            finally
            {
                this.TransactionManagerProvider.GetTransactionManager().RollbackQueryTransaction();
            }
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

        private void ThrowUnathorizedException(HttpActionContext actionContext)
        {
            var host = actionContext.Request.RequestUri.DnsSafeHost;
            actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { ReasonPhrase = ErrorMessages.User_Not_authorized };
            actionContext.Response.Headers.Add("WWW-Authenticate", string.Format("Basic realm=\"{0}\"", host));
        }

        private void ThrowLockedOutException(HttpActionContext actionContext)
        {
            actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { ReasonPhrase = ErrorMessages.UserLockedOut };
        }

        private void ResponseMaintenanceMessage(HttpActionContext actionContext)
        {
            actionContext.Response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) { ReasonPhrase = ErrorMessages.Maintenance };
        }

        private bool Authorize(string username, string password)
        {
            return validateUserCredentials(username, password);
        }

        private bool IsAccountLockedOut()
        {
            return this.userHelper.WebUser == null || this.userHelper.WebUser.MembershipUser.IsLockedOut;
        }
    }
}