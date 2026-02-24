using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Designer.Assistant;
using WB.Core.BoundedContexts.Designer.Assistant.Settings;
using WB.Core.BoundedContexts.Designer.Implementation;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.Infrastructure.PlainStorage;
using WB.UI.Designer.Code;
using WB.UI.Designer.Services;
using Newtonsoft.Json.Linq;

namespace WB.UI.Designer.Controllers.Api.Designer
{
    [Authorize]
    [ApiController]
    [QuestionnairePermissions]
    [Route("api/[controller]")]
    public class AssistanceController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly IModelSettings modelSettings;
        private readonly ILogger<AssistanceController> logger;
        private readonly UserManager<DesignerIdentityUser> userManager;
        private readonly IQuestionnaireHelper questionnaireHelper;
        private readonly IJwtTokenService jwtTokenService;

        private readonly IQuestionnaireContextProvider questionnaireContextProvider;
        private readonly IPlainKeyValueStorage<AssistantSettings> appSettingsStorage;
        private readonly IQuestionnaireAssistant questionnaireAssistant;
        //private readonly IQuestionnaireContextProvider questionnaireContextProvider;

        public AssistanceController(IConfiguration configuration,
            //IQuestionnaireContextProvider questionnaireContextProvider,
            ILogger<AssistanceController> logger,
            IQuestionnaireContextProvider questionnaireContextProvider,
            IQuestionnaireAssistant questionnaireAssistant,
            IPlainKeyValueStorage<AssistantSettings> appSettingsStorage,
            UserManager<DesignerIdentityUser> userManager,
            IQuestionnaireHelper questionnaireHelper,
            IJwtTokenService jwtTokenService)
        {
            this.configuration = configuration;
            //this.questionnaireContextProvider = questionnaireContextProvider;
            this.logger = logger;
            this.questionnaireContextProvider = questionnaireContextProvider;
            this.questionnaireAssistant = questionnaireAssistant;

            this.modelSettings = new AssistantModelSettings(configuration);
            this.appSettingsStorage = appSettingsStorage;
            this.userManager = userManager;
            this.questionnaireHelper = questionnaireHelper;
            this.jwtTokenService = jwtTokenService;
        }

        public class Message
        {
            public string Role { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
        }

        public class AssistanceRequest
        {
            public string Prompt { get; set; } = string.Empty;
            public List<Message> Messages { get; set; } = new List<Message>();
            public Guid? EntityId { get; set; }
            public string Area { get; set; } = string.Empty;
            public Guid? ConversationId { get; set; }
        }

        public class AssistanceReactionRequest
        {
            public Guid? EntityId { get; set; }
            public long? ClientMessageId { get; set; }
            public long? ClientTimestamp { get; set; }
            public string Prompt { get; set; } = string.Empty;
            public string AssistantResponse { get; set; } = string.Empty;
            public long? AssistantCallId { get; set; }
            public AssistantResponseReaction Reaction { get; set; }
            public string? Comment { get; set; }
        }

        public enum AssistantResponseReaction
        {
            None = 0,
            Like = 1,
            Dislike = 2
        }

        public record ReactionRequest(AssistantResponseReaction Reaction, string? Comment = null);

        public class AssistanceResponse
        {
            public string Expression { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public JToken? Meta { get; set; }
            public Guid? ConversationId { get; set; }
        }

        [HttpPost]
        [Route("{id}")]
        public async Task<IActionResult> Post(Guid id, [FromBody] AssistanceRequest request)
        {
            var setting = appSettingsStorage.GetById(AssistantSettings.AssistantSettingsKey);

            var user = await userManager.GetUserAsync(User);

            //check if AI assistant is enabled for current user
            if (setting == null || !setting.IsEnabled)
                return StatusCode(406, "AI assistant is not enabled.");

            if (setting.IsAvailableToAllUsers != true && user?.AssistantEnabled != true)
                return StatusCode(406, "AI assistant is not enabled.");

            if (id == Guid.Empty)
                return BadRequest("Either 'questionnaireId' must be provided.");
            if (!request.EntityId.HasValue)
                return BadRequest("Either 'entityId' must be provided.");

            var questionnaireRevision = questionnaireHelper.GetLastRevision(id);

            try
            {
                var assistantAddress = configuration["Providers:Assistant:AssistantAddress"];
                if (string.IsNullOrWhiteSpace(assistantAddress))
                {
                    return StatusCode(500, "Assistant service address is not configured.");
                }

                var httpClient = new HttpClient();

                var apiKey = configuration["Providers:Assistant:ApiKey"];
                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    httpClient.DefaultRequestHeaders.Add("X-Client-Api-Key", apiKey);
                }

                if (user != null)
                {
                    // forward user id explicitly for downstream auditing/rate-limiting
                    httpClient.DefaultRequestHeaders.Add("X-User-Id", user.Id.ToString());

                    var jwtToken = jwtTokenService.GenerateToken(user);
                    httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwtToken);
                }

                var proxyRequest = new
                {
                    QuestionnaireId = $"{questionnaireRevision.QuestionnaireId}${questionnaireRevision.Version}",
                    EntityId = request.EntityId.Value,
                    Prompt = !string.IsNullOrWhiteSpace(request.Prompt) ? request.Prompt : request.Messages.Last().Content,
                    Messages = request.Messages.SkipLast(1).Select(m => new { m.Role, m.Content }).ToList(),
                    conversationId = request.ConversationId
                };

                var jsonContent = JsonSerializer.Serialize(proxyRequest);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var httpResponse = await httpClient.PostAsync(assistantAddress, httpContent);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    var errorContent = await httpResponse.Content.ReadAsStringAsync();
                    logger.LogError("Assistant service returned error: {StatusCode} - {Content}", httpResponse.StatusCode, errorContent);
                    return StatusCode((int)httpResponse.StatusCode, "Error from assistant service.");
                }

                var responseContent = await httpResponse.Content.ReadAsStringAsync();
                var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);

                long? callLogId = null;
                if (responseData.TryGetProperty("callLogId", out var callLogIdProperty) && callLogIdProperty.TryGetInt64(out var parsedCallLogId))
                {
                    callLogId = parsedCallLogId;
                }

                JToken? metaToken = null;
                if (responseData.TryGetProperty("meta", out var meta))
                {
                    // This controller uses Newtonsoft.Json for MVC serialization (see Startup.AddNewtonsoftJson).
                    // JToken doesn't have the self-referencing Parent loop that System.Text.Json.Nodes has.
                    metaToken = JToken.Parse(meta.GetRawText());
                }

                if (callLogId.HasValue)
                {
                    if (metaToken is not JObject metaObject)
                    {
                        metaObject = new JObject();
                        metaToken = metaObject;
                    }

                    metaObject["callLogId"] = callLogId.Value;
                }

                string? expression = null;
                if (responseData.TryGetProperty("expression", out var expr)) expression = expr.GetString();
                else if (responseData.TryGetProperty("Expression", out var expr2)) expression = expr2.GetString();

                string? message = null;
                if (responseData.TryGetProperty("message", out var msg)) message = msg.GetString();
                else if (responseData.TryGetProperty("Message", out var msg2)) message = msg2.GetString();
                else if (responseData.TryGetProperty("answer", out var ans)) message = ans.GetString();
                else if (responseData.TryGetProperty("Answer", out var ans2)) message = ans2.GetString();

                Guid? conversationId = null;
                if (responseData.TryGetProperty("conversationId", out var conv)
                    && conv.ValueKind == JsonValueKind.String
                    && Guid.TryParse(conv.GetString(), out var parsedConv))
                {
                    conversationId = parsedConv;
                }

                return Ok(new
                {
                    Expression = expression,
                    Message = message,
                    Meta = metaToken,
                    conversationId = conversationId
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return StatusCode(406, "Error communicating with the AI model service. Try again later.");
            }
        }


        [HttpPost]
        [Route("{id}/reaction")]
        public async Task<IActionResult> Reaction(Guid id, [FromBody] AssistanceReactionRequest request)
        {
            var setting = appSettingsStorage.GetById(AssistantSettings.AssistantSettingsKey);
            var user = await userManager.GetUserAsync(User);

            if (setting == null || !setting.IsEnabled)
                return StatusCode(406, "AI assistant is not enabled.");

            if (setting.IsAvailableToAllUsers != true && user?.AssistantEnabled != true)
                return StatusCode(406, "AI assistant is not enabled.");

            if (id == Guid.Empty)
                return BadRequest("Either 'questionnaireId' must be provided.");
            if (!request.EntityId.HasValue)
                return BadRequest("Either 'entityId' must be provided.");
            if (string.IsNullOrWhiteSpace(request.AssistantResponse))
                return BadRequest("'assistantResponse' must be provided.");

            if (!request.AssistantCallId.HasValue || request.AssistantCallId.Value <= 0)
                return BadRequest("'assistantCallId' must be provided.");

            if (!Enum.IsDefined(typeof(AssistantResponseReaction), request.Reaction))
                return BadRequest("'reaction' must be 0 (None), 1 (Like) or 2 (Dislike).");

            var questionnaireRevision = questionnaireHelper.GetLastRevision(id);

            // We don't persist reactions here yet; we log it for downstream collection.
            // Hash is used to avoid dumping full answer text into logs.
            var assistantResponseHash = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(request.AssistantResponse))
            );

            var promptHash = string.IsNullOrWhiteSpace(request.Prompt)
                ? null
                : Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.Prompt)));

            logger.LogInformation(
                "Assistant reaction: userId={UserId} questionnaire={QuestionnaireId}${Version} entityId={EntityId} reaction={Reaction} assistantCallId={AssistantCallId} clientMsgId={ClientMessageId} clientTs={ClientTimestamp} promptHash={PromptHash} responseHash={ResponseHash}",
                user?.Id,
                questionnaireRevision.QuestionnaireId,
                questionnaireRevision.Version,
                request.EntityId,
                request.Reaction,
                request.AssistantCallId,
                request.ClientMessageId,
                request.ClientTimestamp,
                promptHash,
                assistantResponseHash
            );

            try
            {
                var assistantBaseAddress = configuration["Providers:Assistant:AssistantAddress"];
                if (string.IsNullOrWhiteSpace(assistantBaseAddress))
                    return StatusCode(500, "Assistant service address is not configured.");

                // For reaction we call provider API directly.
                // AssistantAddress may be configured either as full chat endpoint or as base URL.
                // We normalize it here to base URL and append the reaction route.
                var baseUri = new Uri(assistantBaseAddress, UriKind.Absolute);
                var reactionUri = new Uri(baseUri, $"/api/v1/AssistantCalls/{request.AssistantCallId.Value}/reaction");

                using var httpClient = new HttpClient();

                var apiKey = configuration["Providers:Assistant:ApiKey"];
                if (!string.IsNullOrWhiteSpace(apiKey))
                    httpClient.DefaultRequestHeaders.Add("X-Client-Api-Key", apiKey);

                if (user != null)
                {
                    httpClient.DefaultRequestHeaders.Add("X-User-Id", user.Id.ToString());
                    var jwtToken = jwtTokenService.GenerateToken(user);
                    httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwtToken);
                }

                var providerRequest = new ReactionRequest(request.Reaction, request.Comment);
                var jsonContent = JsonSerializer.Serialize(providerRequest);
                using var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var httpResponse = await httpClient.PostAsync(reactionUri, httpContent);
                if (!httpResponse.IsSuccessStatusCode)
                {
                    var errorContent = await httpResponse.Content.ReadAsStringAsync();
                    logger.LogError("Assistant reaction endpoint returned error: {StatusCode} - {Content}", httpResponse.StatusCode, errorContent);
                    return StatusCode((int)httpResponse.StatusCode, "Error from assistant service.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                // Don't fail the UI action hard just because reaction collecting is down.
            }

            return Ok();
        }
    }
}
