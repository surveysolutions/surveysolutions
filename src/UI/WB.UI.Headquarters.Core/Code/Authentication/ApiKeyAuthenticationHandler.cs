using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.UI.Headquarters.Code.Authentication
{
    
    public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
    }
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
    {
        private readonly IPlainKeyValueStorage<ExportServiceSettings> exportServiceSettings;

        public ApiKeyAuthenticationHandler(IOptionsMonitor<ApiKeyAuthenticationOptions> options, 
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IPlainKeyValueStorage<ExportServiceSettings> exportServiceSettings) : base(options, logger, encoder, clock)
        {
            this.exportServiceSettings = exportServiceSettings;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string apiKey = GetTokenFromHeader();
            if (string.IsNullOrEmpty(apiKey))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

           //validate key and return identity
           
           return Task.FromResult(AuthenticateResult.NoResult());
            
            var identity = new ClaimsIdentity(Scheme.Name);
            var claimsPrincipal = new ClaimsPrincipal(identity);
            var authenticationTicket = new AuthenticationTicket(claimsPrincipal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(authenticationTicket));
        }
        
        private string GetTokenFromHeader()
        {
            string token = null;
            string authorization = Request.Headers["Authorization"];

            if (string.IsNullOrEmpty(authorization))
            {
                return null;
            }

            if (authorization.StartsWith("ApiKey ", StringComparison.OrdinalIgnoreCase))
            {
                token = authorization.Substring("ApiKey ".Length).Trim();
            }

            return token;
        }
    }
}
