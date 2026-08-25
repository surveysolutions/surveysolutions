using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.MembershipProvider.Roles;
using WB.UI.Designer.Extensions;
using WB.UI.Designer.Services;

namespace WB.UI.Designer.Controllers.Api.Designer
{
    /// <summary>
    /// Thin proxy between the Control Panel import page and the Assistant provider. It carries no AI
    /// logic: it only forwards the source file, polls the operation status and streams the report.
    /// </summary>
    [Authorize(Roles = nameof(SimpleRoleEnum.Administrator))]
    [ApiController]
    [Route("api/ai-questionnaire-import")]
    public class AiQuestionnaireImportController : ControllerBase
    {
        public const long MaxSourceFileSizeBytes = 50L * 1024 * 1024;

        private const string ImportsPath = "/api/v1/questionnaire-imports";

        private readonly IConfiguration configuration;
        private readonly ILogger<AiQuestionnaireImportController> logger;
        private readonly UserManager<DesignerIdentityUser> userManager;
        private readonly IJwtTokenService jwtTokenService;
        private readonly IHttpClientFactory httpClientFactory;

        public AiQuestionnaireImportController(
            IConfiguration configuration,
            ILogger<AiQuestionnaireImportController> logger,
            UserManager<DesignerIdentityUser> userManager,
            IJwtTokenService jwtTokenService,
            IHttpClientFactory httpClientFactory)
        {
            this.configuration = configuration;
            this.logger = logger;
            this.userManager = userManager;
            this.jwtTokenService = jwtTokenService;
            this.httpClientFactory = httpClientFactory;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(MaxSourceFileSizeBytes)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxSourceFileSizeBytes)]
        public async Task<IActionResult> Start(IFormFile? file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Please select a questionnaire file to import." });

            if (file.Length > MaxSourceFileSizeBytes)
                return BadRequest(new { message = "The file exceeds the maximum allowed size of 50 MB." });

            using var content = new MultipartFormDataContent();
            await using var fileStream = file.OpenReadStream();
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(
                string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType);
            content.Add(fileContent, "file", file.FileName);

            return await ForwardAsync(HttpMethod.Post, ImportsPath, content, cancellationToken);
        }

        [HttpGet]
        [Route("{operationId:guid}")]
        public Task<IActionResult> GetStatus(Guid operationId, CancellationToken cancellationToken)
            => ForwardAsync(HttpMethod.Get, $"{ImportsPath}/{operationId}", null, cancellationToken);

        [HttpGet]
        [Route("{operationId:guid}/report")]
        public Task<IActionResult> GetReport(Guid operationId, CancellationToken cancellationToken)
            => ForwardAsync(HttpMethod.Get, $"{ImportsPath}/{operationId}/report", null, cancellationToken);

        private async Task<IActionResult> ForwardAsync(
            HttpMethod method, string path, HttpContent? content, CancellationToken cancellationToken)
        {
            var assistantAddress = configuration["Providers:Assistant:AssistantAddress"];
            if (string.IsNullOrWhiteSpace(assistantAddress)
                || !Uri.TryCreate(assistantAddress, UriKind.Absolute, out var assistantUri))
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Assistant service address is not configured." });
            }

            var requestUri = new Uri(new Uri(assistantUri.GetLeftPart(UriPartial.Authority)), path);

            using var request = new HttpRequestMessage(method, requestUri) { Content = content };

            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            request.Headers.TryAddWithoutValidation("X-User-Id", user.Id.ToString());

            try
            {
                var jwtToken = jwtTokenService.GenerateToken(user);
                if (!string.IsNullOrWhiteSpace(jwtToken))
                    request.Headers.TryAddWithoutValidation("Authorization", "Bearer " + jwtToken);
            }
            catch (InvalidOperationException exception)
            {
                logger.LogError(exception, "Assistant JWT secret key is not configured; questionnaire import cannot run.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Assistant service authentication is not configured." });
            }

            var httpClient = httpClientFactory.CreateClient(
                AssistantProviderHttpClientExtensions.AssistantProviderHttpClientName);

            try
            {
                using var response = await httpClient.SendAsync(
                    request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                var responseBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/json";
                var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
                               ?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"');

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning("Assistant questionnaire import returned {StatusCode} for {Path}.",
                        response.StatusCode, path);
                }

                if (fileName != null && response.IsSuccessStatusCode)
                    return File(responseBytes, contentType, fileName);

                return new ContentResult
                {
                    Content = System.Text.Encoding.UTF8.GetString(responseBytes),
                    ContentType = contentType,
                    StatusCode = (int)response.StatusCode
                };
            }
            catch (OperationCanceledException)
            {
                return StatusCode(499);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Error communicating with the Assistant questionnaire import service.");
                return StatusCode(StatusCodes.Status502BadGateway,
                    new { message = "Assistant service is unavailable. Try again later." });
            }
        }
    }
}
