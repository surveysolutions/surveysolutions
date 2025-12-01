using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Designer.Assistant;
using WB.Core.BoundedContexts.Designer.Assistant.Settings;

namespace WB.UI.Designer.Controllers.Api.Designer
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AssistanceController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly IModelSettings modelSettings;
        private readonly ILogger<AssistanceController> logger;

        private readonly IQuestionnaireContextProvider questionnaireContextProvider;

        private readonly IQuestionnaireAssistant questionnaireAssistant;
        //private readonly IQuestionnaireContextProvider questionnaireContextProvider;

        public AssistanceController(IConfiguration configuration, 
            //IQuestionnaireContextProvider questionnaireContextProvider,
            ILogger<AssistanceController> logger,
            IQuestionnaireContextProvider questionnaireContextProvider,
            IQuestionnaireAssistant questionnaireAssistant)
        {
            this.configuration = configuration;
            //this.questionnaireContextProvider = questionnaireContextProvider;
            this.logger = logger;
            this.questionnaireContextProvider = questionnaireContextProvider;
            this.questionnaireAssistant = questionnaireAssistant;

            // Switch between OpenAIModelSettings and LlamaModelSettings as needed
            // Example: this.modelSettings = new OpenAIModelSettings(configuration);
            this.modelSettings = new LlamaModelSettings();
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
            public Guid? QuestionnaireId { get; set; }
            public Guid? EntityId { get; set; }
            public string Area { get; set; } = string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AssistanceRequest request)
        {
            if (modelSettings is OpenAIModelSettings && string.IsNullOrWhiteSpace(modelSettings.ApiKey))
                return BadRequest("OpenAI API key is not configured.");

            if (!request.QuestionnaireId.HasValue)
                return  BadRequest("Either 'questionnaireId' must be provided.");
            if (!request.EntityId.HasValue)
                return  BadRequest("Either 'entityId' must be provided.");
            
            try
            {
                var response = await questionnaireAssistant.GetResponseAsync(new AssistantRequest(
                    request.QuestionnaireId.Value, request.EntityId.Value, request.Prompt,
                    request.Messages.Select(m => new AssistantMessage(m.Role, m.Content)).ToList()
                ));

                return Ok(response.Answer);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return StatusCode(406, "Error communicating with the AI model service. Try again later.");
            }
        }

        
    }
}
