using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WB.UI.Designer.Services
{
    public class WebTesterService : IWebTesterService
    {
        private readonly IOneTimeCodeStore codeStore;
        private readonly WebTesterSettings settings;
        private readonly ILogger<WebTesterService> logger;

        public WebTesterService(
            IOneTimeCodeStore codeStore,
            IOptions<WebTesterSettings> settings,
            ILogger<WebTesterService> logger)
        {
            this.codeStore = codeStore;
            this.settings = settings.Value;
            this.logger = logger;

            if (string.IsNullOrWhiteSpace(this.settings.ServiceApiKey))
            {
                logger.LogWarning(
                    "WebTester:ServiceApiKey is not configured — " +
                    "service-to-service key validation is disabled. " +
                    "Configure WebTester:ServiceApiKey in both Designer and WebTester for production.");
            }
        }

        public async Task<string> CreateOneTimeCodeAsync(
            Guid questionnaireId,
            string? userId,
            string correlationId,
            CancellationToken ct = default)
        {
            var ttl = settings.CodeTtlSeconds > 0 ? settings.CodeTtlSeconds : 60;
            var now = DateTime.UtcNow;
            var code = GenerateSecureCode();

            var entity = new OneTimeCodeEntity
            {
                Code            = code,
                UserId          = userId,
                CorrelationId   = correlationId,
                TargetService   = "WB.WebTester",
                QuestionnaireId = questionnaireId,
                CreatedAt       = now,
                ExpiresAt       = now.AddSeconds(ttl),
                Used            = false
            };

            await codeStore.SaveAsync(entity, ct);

            logger.LogInformation(
                "One-time code created. CorrelationId={CorrelationId}, UserId={UserId}, " +
                "TargetService={TargetService}, ExpiresAt={ExpiresAt}",
                correlationId, userId ?? "anonymous", entity.TargetService, entity.ExpiresAt);

            return code;
        }

        private static string GenerateSecureCode()
        {
            var bytes = RandomNumberGenerator.GetBytes(32); // 256-bit entropy
            return Convert.ToBase64String(bytes)
                .Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }
    }
}
