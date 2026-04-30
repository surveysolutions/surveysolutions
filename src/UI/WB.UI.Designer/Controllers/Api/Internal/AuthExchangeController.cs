using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
    /// Service authentication is enforced inside the action via X-Service-Name / X-Service-Key
    /// header validation with constant-time comparison.
    /// </summary>
    [Route("api/internal/auth")]
    [AllowAnonymous]
    public class AuthExchangeController : ControllerBase
    {
        private const string ServiceNameHeader = "X-Service-Name";
        private const string ServiceKeyHeader  = "X-Service-Key";

        /// <summary>
        /// Hard ceiling on accepted code length.  Legitimate codes are 43-character
        /// base64url strings; 128 gives room for future changes while still bounding
        /// the size of attacker-supplied input before it reaches the cache.
        /// </summary>
        private const int MaxCodeLength = 128;

        /// <summary>
        /// The only service identity that is authorised to call the exchange endpoint.
        /// Checked with an ordinal, case-insensitive comparison so that casing
        /// differences in configuration files do not accidentally open the door to
        /// unrelated service names.
        /// </summary>
        public const string ExpectedServiceName = "WB.WebTester";

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
            [FromHeader(Name = ServiceNameHeader)] string? serviceName,
            [FromHeader(Name = ServiceKeyHeader)]  string? serviceKey,
            CancellationToken ct)
        {
            // 1. Authenticate Service B — reject immediately when headers are absent
            if (string.IsNullOrEmpty(serviceName) || string.IsNullOrEmpty(serviceKey))
                return Unauthorized(new { error = "Invalid service credentials" });

            if (!IsValidServiceCredentials(serviceName, serviceKey))
            {
                logger.LogWarning(
                    "Rejected exchange attempt from unknown service. ServiceName={ServiceName}",
                    serviceName);
                return Unauthorized(new { error = "Invalid service credentials" });
            }

            // 2. Basic input validation — length and charset are checked before the
            //    code is used as a cache key so that over-large or malformed values
            //    are rejected cheaply without stressing the backing store.
            if (string.IsNullOrWhiteSpace(request.Code))
                return BadRequest(new { error = "Code is required" });

            if (request.Code.Length > MaxCodeLength)
                return BadRequest(new { error = $"Code must not exceed {MaxCodeLength} characters" });

            if (!IsValidCodeFormat(request.Code))
                return BadRequest(new { error = "Code contains invalid characters" });

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

        /// <summary>
        /// Returns true when every character in <paramref name="code"/> belongs to the
        /// base64url alphabet (A-Z a-z 0-9 _ -), i.e. exactly the characters produced
        /// by <see cref="WebTesterService.GenerateSecureCode"/>.  No heap allocation;
        /// iterates the string once.
        /// </summary>
        private static bool IsValidCodeFormat(string code)
        {
            foreach (var ch in code)
            {
                if (!((ch >= 'A' && ch <= 'Z') ||
                      (ch >= 'a' && ch <= 'z') ||
                      (ch >= '0' && ch <= '9') ||
                      ch == '-' || ch == '_'))
                    return false;
            }
            return true;
        }

        private bool IsValidServiceCredentials(string serviceName, string serviceKey)
        {
            const int minimumServiceApiKeyLength = 32;

            if (string.IsNullOrWhiteSpace(settings.ServiceApiKey))
            {
                logger.LogError(
                    "Exchange failed: WebTester:ServiceApiKey is not configured. " +
                    "ServiceName={ServiceName}", serviceName);
                throw new InvalidOperationException("WebTester:ServiceApiKey must be configured.");
            }

            if (settings.ServiceApiKey.Length < minimumServiceApiKeyLength)
            {
                logger.LogError(
                    "Exchange failed: WebTester:ServiceApiKey is shorter than the required minimum length of {MinimumLength}. " +
                    "ConfiguredLength={ConfiguredLength}, ServiceName={ServiceName}",
                    minimumServiceApiKeyLength, settings.ServiceApiKey.Length, serviceName);
                throw new InvalidOperationException(
                    $"WebTester:ServiceApiKey must be at least {minimumServiceApiKeyLength} characters long.");
            }
            // Bind the presented service name to the single authorised identity so that
            // a valid key cannot be reused by an unrelated future service.
            if (!string.Equals(serviceName, ExpectedServiceName, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogWarning(
                    "Exchange rejected: unexpected service name. " +
                    "Expected={Expected}, Got={Got}",
                    ExpectedServiceName, serviceName);
                return false;
            }

            if (string.IsNullOrEmpty(serviceKey))
                return false;

            // Reject impossible candidates before allocating a byte[] for the presented
            // header value. This avoids large attacker-controlled allocations while
            // preserving the constant-time comparison for equal-length candidates.
            var expectedBytes = Encoding.UTF8.GetBytes(settings.ServiceApiKey);
            var presentedByteCount = Encoding.UTF8.GetByteCount(serviceKey);
            if (presentedByteCount != expectedBytes.Length)
                return false;

            // Constant-time byte comparison to prevent timing side-channels that
            // could otherwise be used to brute-force the shared secret one byte at a time.
            var presentedBytes = Encoding.UTF8.GetBytes(serviceKey);
            return CryptographicOperations.FixedTimeEquals(expectedBytes, presentedBytes);
        }
    }
}

