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
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Designer.Assistant;
using WB.Core.BoundedContexts.Designer.Assistant.Settings;
using WB.Core.BoundedContexts.Designer.Implementation;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.Infrastructure.PlainStorage;

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
            UserManager<DesignerIdentityUser> userManager)
        {
            this.configuration = configuration;
            //this.questionnaireContextProvider = questionnaireContextProvider;
            this.logger = logger;
            this.questionnaireContextProvider = questionnaireContextProvider;
            this.questionnaireAssistant = questionnaireAssistant;
             
            this.modelSettings = new AssistantModelSettings(configuration);
            this.appSettingsStorage = appSettingsStorage;
            this.userManager = userManager;
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
        }

        [HttpPost]
        [Route("{id}")]
        public async Task<IActionResult> Post(Guid id, [FromBody] AssistanceRequest request)
        {
            var setting = appSettingsStorage.GetById(AssistantSettings.AssistantSettingsKey);
            
            var user = await userManager.GetUserAsync(User);
                
            //check if AI assistant is enabled for current user
            if(setting == null || !setting.IsEnabled )
                return  StatusCode(406, "AI assistant is not enabled.");
            
            if(setting.IsAvailableToAllUsers != true && user?.AssistantEnabled != true)
                return  StatusCode(406, "AI assistant is not enabled.");

            if (id == Guid.Empty)
                return  BadRequest("Either 'questionnaireId' must be provided.");
            if (!request.EntityId.HasValue)
                return  BadRequest("Either 'entityId' must be provided.");
            
            try
            {
                var assistantAddress = configuration["Providers:Assistant:AssistantAddress"];
                if (string.IsNullOrWhiteSpace(assistantAddress))
                {
                    return StatusCode(500, "Assistant service address is not configured.");
                }

                var httpClient = new HttpClient();
                
                var proxyRequest = new
                {
                    QuestionnaireId = id,
                    EntityId = request.EntityId.Value,
                    Prompt = !string.IsNullOrWhiteSpace(request.Prompt) ? request.Prompt : request.Messages.Last().Content,
                    Messages = request.Messages.SkipLast(1).Select(m => new { m.Role, m.Content }).ToList()
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

                return Ok(new
                {
                    Expression = responseData.TryGetProperty("expression", out var expr) ? expr.GetString() : null,
                    Message = responseData.TryGetProperty("message", out var msg) ? msg.GetString() : null
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return StatusCode(406, "Error communicating with the AI model service. Try again later.");
            }
        }
    }
}
