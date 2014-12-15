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
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Code
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class ApiBasicAuthAttribute : AuthorizationFilterAttribute
    {
        private ILocalizationService localizationService
        {
            get { return ServiceLocator.Current.GetInstance<ILocalizationService>(); }
        }

        private readonly Func<string, string, bool> validateUserCredentials;

        private static readonly string[] emptyArray = new string[0];

        private string[] rolesSplit = emptyArray;
        private string roles;

        public string Roles
        {
            get { return roles ?? string.Empty; }
            set
            {
                roles = value;
                rolesSplit = SplitString(value);
            }
        }

        internal static string[] SplitString(string original)
        {
            if (string.IsNullOrEmpty(original))
                return emptyArray;

            return original.Split(',');
        }


        public ApiBasicAuthAttribute() : this(Membership.ValidateUser)
        {
        }

        internal ApiBasicAuthAttribute(Func<string, string, bool> validateUserCredentials)
        {
            this.validateUserCredentials = validateUserCredentials;
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            BasicCredentials credentials = ParseCredentials(actionContext);
            if (credentials == null)
            {
                Challenge(actionContext);
                return;
            }

            if (!Authorize(credentials.Username, credentials.Password))
            {
                Challenge(actionContext);
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

        private void Challenge(HttpActionContext actionContext)
        {
            string host = actionContext.Request.RequestUri.DnsSafeHost;
            actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                ReasonPhrase = this.localizationService.GetString("InvalidUser")
            };
            actionContext.Response.Headers.Add("WWW-Authenticate", string.Format("Basic realm=\"{0}\"", host));
        }

        private bool Authorize(string username, string password)
        {
            return validateUserCredentials(username, password);
        }
    }
}