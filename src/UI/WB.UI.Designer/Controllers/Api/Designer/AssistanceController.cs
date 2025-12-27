using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
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
            public Guid? QuestionnaireId { get; set; }
            public Guid? EntityId { get; set; }
            public string Area { get; set; } = string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AssistanceRequest request)
        {
            var setting = appSettingsStorage.GetById(AssistantSettings.AssistantSettingsKey);
            
            var user = await userManager.GetUserAsync(User);
                
            //check if AI assistant is enabled for current user
            if(setting == null || !setting.IsEnabled )
                return  StatusCode(406, "AI assistant is not enabled.");
            
            if(setting.IsAvailableToAllUsers != true && user?.AssistantEnabled != true)
                return  StatusCode(406, "AI assistant is not enabled.");

            if (!request.QuestionnaireId.HasValue)
                return  BadRequest("Either 'questionnaireId' must be provided.");
            if (!request.EntityId.HasValue)
                return  BadRequest("Either 'entityId' must be provided.");
            
            try
            {
                var response = await questionnaireAssistant.GetResponseAsync(new AssistantRequest(
                    request.QuestionnaireId.Value, 
                    request.EntityId.Value, 
                    !string.IsNullOrWhiteSpace(request.Prompt) ? request.Prompt : request.Messages.Last().Content,
                    request.Messages.SkipLast(1).Select(m => new AssistantMessage(m.Role, m.Content)).ToList()
                ),
                this.modelSettings);

                return Ok(new
                {
                    response.Expression,
                    response.Message
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
