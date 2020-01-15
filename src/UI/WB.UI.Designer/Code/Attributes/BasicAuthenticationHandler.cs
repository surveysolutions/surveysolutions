using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WB.UI.Designer.Code.Attributes
{
    public class BasicAuthenticationHandler : AuthenticationHandler<BasicAuthenticationSchemeOptions>
    {
        private readonly IBasicAuthenticationService _userService;
        private UnauthorizedException loginException;

        public BasicAuthenticationHandler(
            IOptionsMonitor<BasicAuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IBasicAuthenticationService userService)
            : base(options, logger, encoder, clock)
        {
            _userService = userService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.NoResult();

            ClaimsPrincipal principal;
            try 
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':');
                var username = credentials[0];
                var password = credentials[1];
                BasicCredentials creds = new BasicCredentials
                {
                    Username = username, 
                    Password = password
                };

                principal = await _userService.AuthenticateAsync(creds);
            } 
            catch (UnauthorizedException e)
            {
                this.loginException = e;
                return AuthenticateResult.Fail(e);
            }

            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            await HandleFail();
        }

        protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            await HandleFail();
        }

        private async Task HandleFail()
        {
            Response.Headers["WWW-Authenticate"] = $"Basic realm=\"{Options.Realm}\", charset=\"UTF-8\"";

            if (this.loginException != null)
            {
                Response.StatusCode = loginException.ResponseStatusCode;
                var feature = Response.HttpContext?.Features?.Get<IHttpResponseFeature>();
                if (feature != null)
                    feature.ReasonPhrase = loginException.Message;

                await Response.Body.WriteAsync(Encoding.UTF8.GetBytes(this.loginException.Message));
            }
        }
    }
}
