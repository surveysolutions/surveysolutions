using System.Configuration;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.UI.Headquarters.Code.Authentication
{
    public class TenantTokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IPlainKeyValueStorage<ExportServiceSettings> exportServiceSettings;

        public TenantTokenAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, 
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IPlainKeyValueStorage<ExportServiceSettings> exportServiceSettings) : base(options, logger, encoder, clock)
        {
            this.exportServiceSettings = exportServiceSettings;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!AuthenticationHeaderValue.TryParse(Request.Headers[HeaderNames.Authorization].FirstOrDefault(), 
                out AuthenticationHeaderValue header))
                return Task.FromResult(AuthenticateResult.NoResult());

            var appSetting = exportServiceSettings.GetById(AppSetting.ExportServiceStorageKey);
            if (appSetting == null)
            {
                throw new ConfigurationErrorsException("Missing required configuration setting");
            }

            if (header.Parameter != appSetting.Key)
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid auth token"));
            }

            var identity = new ClaimsIdentity(Scheme.Name);
            var claimsPrincipal = new ClaimsPrincipal(identity);
            var authenticationTicket = new AuthenticationTicket(claimsPrincipal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(authenticationTicket));
        }
    }
}
