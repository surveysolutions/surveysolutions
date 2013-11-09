namespace WB.UI.Designer.Code.MessageHandlers
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using System.Security.Principal;
    using System.Threading;
    using System.Web;
    using System.Web.Security;

    public class BasicAuthMessageHandler : DelegatingHandler
    {
        //private readonly IServiceLocator serviceLocator;

        /*public BasicAuthMessageHandler(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }*/

        protected bool Authorize(string username, string password)
        {
            return Membership.ValidateUser(username, password);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            if (request.Headers.Authorization != null && request.Headers.Authorization.Scheme == "Basic")
            {
                var credentials = ParseCredentials(request.Headers.Authorization);

                if (Authorize(credentials.Username, credentials.Password))
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