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

namespace WB.UI.Designer.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class ApiBasicAuthAttribute : AuthorizationFilterAttribute
    {
        private readonly Func<string, string, bool> validateUserCredentials;

        public ApiBasicAuthAttribute()
            : this(Membership.ValidateUser){}

        internal ApiBasicAuthAttribute(Func<string, string, bool> validateUserCredentials)
        {
            this.validateUserCredentials = validateUserCredentials;
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var credentials = ParseCredentials(actionContext);
            if (credentials == null)
            {
                this.Challenge(actionContext);
                return;
            }

            if (!this.Authorize(credentials.Username, credentials.Password))
            {
                this.Challenge(actionContext);
                return;
            }

            var identity = new GenericIdentity(credentials.Username, "Basic");
            var principal = new GenericPrincipal(identity, null);

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

        private void Challenge(HttpActionContext actionContext)
        {
            var host = actionContext.Request.RequestUri.DnsSafeHost;
            actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
            actionContext.Response.Headers.Add("WWW-Authenticate", string.Format("Basic realm=\"{0}\"", host));
        }

        private bool Authorize(string username, string password)
        {
            return validateUserCredentials(username, password);
        }
    }
}