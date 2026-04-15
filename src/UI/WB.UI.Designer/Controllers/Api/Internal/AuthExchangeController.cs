using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WB.UI.Designer.Models;
using WB.UI.Designer.Services;

namespace WB.UI.Designer.Controllers.Api.Internal
{

    /// <summary>
    /// Backend-to-backend endpoint: WebTester exchanges a one-time code for a
    /// short-lived delegated JWT it can use to call Designer's /api/webtester/* endpoints.
    /// The browser never sees this exchange.
    /// </summary>
    [Route("api/internal/auth")]
    [ApiController]
    public class AuthExchangeController : ControllerBase
    {
        private const string ServiceNameHeader = "X-Service-Name";
        private const string ServiceKeyHeader = "X-Service-Key";

        private readonly IOneTimeCodeStore codeStore;
        private readonly IDelegatedTokenService delegatedTokenService;
        private readonly WebTesterSettings settings;
        private readonly ILogger<AuthExchangeController> logger;

        public AuthExchangeController(
            IOneTimeCodeStore codeStore,
            IDelegatedTokenService delegatedTokenService,
            IOptions<WebTesterSettings> settings,
            ILogger<AuthExchangeController> logger)
        {
            this.codeStore = codeStore;
            this.delegatedTokenService = delegatedTokenService;
            this.settings = settings.Value;
            this.logger = logger;
        }

        [HttpPost("exchange")]
        public async Task<IActionResult> Exchange(
            [FromBody] ExchangeCodeRequest request,
            CancellationToken ct)
        {
            // 1. Authenticate Service B
            var serviceName = Request.Headers[ServiceNameHeader].ToString();
            var serviceKey  = Request.Headers[ServiceKeyHeader].ToString();

            if (!IsValidServiceCredentials(serviceName, serviceKey))
            {
                logger.LogWarning(
                    "Rejected exchange attempt from unknown service. ServiceName={ServiceName}",
                    serviceName);
                return Unauthorized(new { error = "Invalid service credentials" });
            }

            // 2. Basic input validation
            if (string.IsNullOrWhiteSpace(request.Code))
                return BadRequest(new { error = "Code is required" });

            // 3. Look up code
            var entity = await codeStore.GetAsync(request.Code, ct);
            if (entity == null)
            {
                logger.LogWarning(
                    "Exchange failed: unknown code. ServiceName={ServiceName}",
                    serviceName);
                return BadRequest(new { error = "Invalid code" });
            }

            // 4. Expiry check
            if (DateTime.UtcNow > entity.ExpiresAt)
            {
                logger.LogWarning(
                    "Exchange failed: code expired. CorrelationId={CorrelationId}, UserId={UserId}",
                    entity.CorrelationId, entity.UserId ?? "anonymous");
                return StatusCode(410, new { error = "Code expired" });
            }

            // 5. TargetService check
            if (!string.Equals(entity.TargetService, serviceName, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogWarning(
                    "Exchange failed: code issued for different service. " +
                    "Expected={Expected}, Got={Got}, CorrelationId={CorrelationId}",
                    entity.TargetService, serviceName, entity.CorrelationId);
                return StatusCode(403, new { error = "Code was not issued for this service" });
            }

            // 6. Atomic mark-as-used (race-safe)
            var marked = await codeStore.TryMarkAsUsedAsync(request.Code, DateTime.UtcNow, ct);
            if (!marked)
            {
                logger.LogWarning(
                    "Exchange failed: code already used. CorrelationId={CorrelationId}, UserId={UserId}",
                    entity.CorrelationId, entity.UserId ?? "anonymous");
                return Conflict(new { error = "Code already used" });
            }

            // 7. Issue short-lived delegated JWT
            var jwtToken = delegatedTokenService.CreateDelegatedToken(new DelegatedTokenRequest
            {
                UserId          = entity.UserId,
                CorrelationId   = entity.CorrelationId,
                QuestionnaireId = entity.QuestionnaireId,
                AuthorizedParty = serviceName,
                Scope           = "webtester"
            });

            logger.LogInformation(
                "Exchange succeeded. CorrelationId={CorrelationId}, UserId={UserId}, " +
                "ServiceName={ServiceName}, QuestionnaireId={QuestionnaireId}",
                entity.CorrelationId,
                entity.UserId ?? "anonymous",
                serviceName,
                entity.QuestionnaireId);

            var expiresIn = (settings.DelegatedJwtExpirationMinutes > 0
                ? settings.DelegatedJwtExpirationMinutes
                : 10) * 60;

            return Ok(new ExchangeCodeResponse
            {
                AccessToken     = jwtToken,
                ExpiresIn       = expiresIn,
                UserId          = entity.UserId,
                CorrelationId   = entity.CorrelationId,
                QuestionnaireId = entity.QuestionnaireId.ToString()
            });
        }

        private bool IsValidServiceCredentials(string serviceName, string serviceKey)
        {
            // If no ServiceApiKey is configured, skip key validation (dev / single-machine mode).
            // The startup warning is logged by WebTesterService; no per-request log here.
            if (string.IsNullOrWhiteSpace(settings.ServiceApiKey))
                return !string.IsNullOrWhiteSpace(serviceName);

            return !string.IsNullOrWhiteSpace(serviceName)
                && string.Equals(serviceKey, settings.ServiceApiKey, StringComparison.Ordinal);
        }
    }
}

