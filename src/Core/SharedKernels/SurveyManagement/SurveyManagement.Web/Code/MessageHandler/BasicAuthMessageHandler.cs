using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Code.MessageHandler
{
    public class BasicAuthMessageHandler : DelegatingHandler
    {
        protected bool Authorize(string username, string password)
        {
            return Membership.ValidateUser(username, password);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            if (request.Headers.Authorization != null && request.Headers.Authorization.Scheme == "Basic")
            {
                var credentials = ParseCredentials(request.Headers.Authorization);

                if (this.Authorize(credentials.Username, credentials.Password))
                {
                    var identity = new GenericIdentity(credentials.Username, "Basic");
                    var principal = new GenericPrincipal(identity, null);

                    Thread.CurrentPrincipal = principal;
                    if (HttpContext.Current != null)
                    {
                        HttpContext.Current.User = principal;
                    }

                    return base.SendAsync(request, cancellationToken);
                }
            }

            return Task<HttpResponseMessage>.Factory.StartNew(
                () =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);

                    response.Headers.Add("WWW-Authenticate", string.Format("Basic realm=\"{0}\"", request.RequestUri.DnsSafeHost));

                    return response;
                });
        }

        private static BasicCredentials ParseCredentials(AuthenticationHeaderValue authHeader)
        {
            try
            {
                var credentials = Encoding.ASCII.GetString(Convert.FromBase64String(authHeader.ToString().Substring(6)));
                int splitOn = credentials.IndexOf(':');

                return new BasicCredentials
                {
                    Username = credentials.Substring(0, splitOn),
                    Password = credentials.Substring(splitOn + 1)
                };
            }
            catch { }

            return new BasicCredentials();
        }

        internal struct BasicCredentials
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }
}