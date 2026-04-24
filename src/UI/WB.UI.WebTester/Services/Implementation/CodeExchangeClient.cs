using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WB.UI.WebTester.Services.Implementation
{
    /// <summary>
    /// Performs the backend-to-backend code exchange:
    ///   WebTester (B) → Designer (A): POST /api/internal/auth/exchange
    /// Credentials are pre-shared service secrets sent in X-Service-Name / X-Service-Key headers.
    /// </summary>
    public class CodeExchangeClient : ICodeExchangeClient
    {
        private readonly IDesignerWebTesterApi designerApi;
        private readonly IOptions<TesterConfiguration> config;
        private readonly ILogger<CodeExchangeClient> logger;

        public CodeExchangeClient(
            IDesignerWebTesterApi designerApi,
            IOptions<TesterConfiguration> config,
            ILogger<CodeExchangeClient> logger)
        {
            this.designerApi = designerApi;
            this.config = config;
            this.logger = logger;
        }

        public async Task<ExchangeCodeResponse?> ExchangeAsync(string code, CancellationToken ct = default)
        {
            var cfg = config.Value;
            var serviceName = cfg.ServiceName ?? "WB.WebTester";
            var serviceKey  = cfg.ServiceApiKey ?? "";

            // Guard: an empty key would cause Designer to reject every exchange with 401.
            // Fail here with a clear message rather than letting the error surface as a
            // confusing "code exchange failed" redirect on the user-facing side.
            if (string.IsNullOrWhiteSpace(serviceKey))
            {
                logger.LogError(
                    "Code exchange aborted: ServiceApiKey is not configured. " +
                    "Every exchange will be rejected by Designer. " +
                    "Set ServiceApiKey in appsettings.ini or via environment variables.");
                return null;
            }

            // Log only code length — never log the secret value itself.
            logger.LogInformation(
                "Initiating code exchange. CodeLength={CodeLength}, ServiceName={ServiceName}",
                code.Length, serviceName);

            try
            {
                var result = await designerApi.ExchangeCodeAsync(
                    new ExchangeCodeRequest { Code = code },
                    serviceName,
                    serviceKey,
                    ct);

                logger.LogInformation(
                    "Code exchange succeeded. UserId={UserId}, CorrelationId={CorrelationId}",
                    result.UserId ?? "anonymous", result.CorrelationId);

                return result;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Code exchange failed. ServiceName={ServiceName}", serviceName);
                return null;
            }
        }
    }
}
