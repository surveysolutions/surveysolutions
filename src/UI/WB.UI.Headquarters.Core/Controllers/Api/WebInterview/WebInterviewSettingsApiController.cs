using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.Headquarters.Controllers.Api.WebInterview
{
    [Route("api/v1/webInterviewSettings")]
    [Authorize(Roles = "Administrator, Headquarter")]
    [ResponseCache(NoStore = true)]
    public class WebInterviewSettingsApiController : ControllerBase
    {
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IWebInterviewConfigProvider webInterviewConfigProvider;


        public WebInterviewSettingsApiController(
            IWebInterviewConfigProvider webInterviewConfigProvider,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory)
        {
            this.webInterviewConfigProvider = webInterviewConfigProvider;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
        }

        public class UpdatePageTemplateModel
        {
            [Required] public WebInterviewUserMessages TitleType { get; set; }
            [Required] public string TitleText { get; set; }
            [Required] public WebInterviewUserMessages MessageType { get; set; }
            public string MessageText { get; set; }

            public WebInterviewUserMessages? ButtonType { get; set; }
            public string ButtonText { get; set; }
        }

        [Route(@"{id}/pageTemplate")]
        [HttpPost]
        public IActionResult UpdatePageTemplate(string id, [FromBody]UpdatePageTemplateModel updateModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!QuestionnaireIdentity.TryParse(id, out var questionnaireIdentity))
                return NotFound();

            QuestionnaireBrowseItem questionnaire = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);
            if (questionnaire == null)
                return NotFound();

            var config = this.webInterviewConfigProvider.Get(questionnaireIdentity);
            config.CustomMessages[updateModel.TitleType] = updateModel.TitleText;
            config.CustomMessages[updateModel.MessageType] = updateModel.MessageText;
            if(updateModel.ButtonType != null && !string.IsNullOrEmpty(updateModel.ButtonText))
                config.CustomMessages[updateModel.ButtonType.Value] = updateModel.ButtonText;
            this.webInterviewConfigProvider.Store(questionnaireIdentity, config);

            return Ok();
        }

        public class UpdateEmailTemplateModel
        {
            [Required] public EmailTextTemplateType Type { get; set; }
            [Required] public string Subject { get; set; }
            [Required] public string Message { get; set; }
            public string PasswordDescription { get; set; }
            public string LinkText { get; set; }
        }

        [Route(@"{id}/emailTemplate")]
        [HttpPost]
        public IActionResult UpdateEmailTemplate(string id, [FromBody]UpdateEmailTemplateModel updateModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!QuestionnaireIdentity.TryParse(id, out var questionnaireIdentity))
                return NotFound();

            QuestionnaireBrowseItem questionnaire = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);
            if (questionnaire == null)
                return NotFound();

            var config = this.webInterviewConfigProvider.Get(questionnaireIdentity);
            config.EmailTemplates[updateModel.Type] = new EmailTextTemplate(updateModel.Subject, updateModel.Message, updateModel.PasswordDescription, updateModel.LinkText);
            this.webInterviewConfigProvider.Store(questionnaireIdentity, config);

            return Ok();
        }

        public class UpdatePageMessageModel
        {
            [Required] public WebInterviewUserMessages Type { get; set; }
            [Required] public string Message { get; set; }
        }

        [Route(@"{id}/pageMessage")]
        [HttpPost]
        public IActionResult UpdatePageMessage(string id, [FromBody]UpdatePageMessageModel updateModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!QuestionnaireIdentity.TryParse(id, out var questionnaireIdentity))
                return NotFound();

            QuestionnaireBrowseItem questionnaire = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);
            if (questionnaire == null)
                return NotFound();

            var config = this.webInterviewConfigProvider.Get(questionnaireIdentity);
            config.CustomMessages[updateModel.Type] = updateModel.Message;
            this.webInterviewConfigProvider.Store(questionnaireIdentity, config);

            return Ok();
        }


        public class UpdateAdditionalSettingsModel
        {
            public bool SpamProtection { get; set; } 
            public int? ReminderAfterDaysIfNoResponse { get; set; } 
            public int? ReminderAfterDaysIfPartialResponse { get; set; } 
            public bool SingleResponse { get; set; }
            public bool EmailOnComplete { get; set; }
            public bool AttachAnswersInEmail { get; set; }
            public bool AllowSwitchToCawiForInterviewer { get;set; }
        }

        [Route(@"{id}/additionalSettings")]
        [HttpPost]
        public IActionResult UpdateAdditionalSettings(string id, [FromBody]UpdateAdditionalSettingsModel updateModel)
        {
            if (!QuestionnaireIdentity.TryParse(id, out var questionnaireIdentity))
                return NotFound();

            QuestionnaireBrowseItem questionnaire = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);
            if (questionnaire == null)
                return NotFound();

            var config = this.webInterviewConfigProvider.Get(questionnaireIdentity);
            config.UseCaptcha = updateModel.SpamProtection;
            config.ReminderAfterDaysIfNoResponse = updateModel.ReminderAfterDaysIfNoResponse;
            config.ReminderAfterDaysIfPartialResponse = updateModel.ReminderAfterDaysIfPartialResponse;
            config.SingleResponse = updateModel.SingleResponse;
            config.EmailOnComplete = updateModel.EmailOnComplete;
            config.AttachAnswersInEmail = updateModel.AttachAnswersInEmail;
            config.AllowSwitchToCawiForInterviewer = updateModel.AllowSwitchToCawiForInterviewer;

            this.webInterviewConfigProvider.Store(questionnaireIdentity, config);

            return Ok();
        }

        [Route(@"{id}/start")]
        [HttpPost]
        public IActionResult Start(string id)
        {
            if (!QuestionnaireIdentity.TryParse(id, out var questionnaireIdentity))
                return NotFound();

            QuestionnaireBrowseItem questionnaire = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);
            if (questionnaire == null)
                return NotFound();

            var config = this.webInterviewConfigProvider.Get(questionnaireIdentity);
            if (config.Started)
                return Ok();

            config.Started = true;
            this.webInterviewConfigProvider.Store(questionnaireIdentity, config);

            return Ok();
        }

        [Route(@"{id}/stop")]
        [HttpPost]
        public IActionResult Stop(string id)
        {
            if (!QuestionnaireIdentity.TryParse(id, out var questionnaireIdentity))
                return NotFound();

            QuestionnaireBrowseItem questionnaire = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);
            if (questionnaire == null)
                return NotFound();

            var config = this.webInterviewConfigProvider.Get(questionnaireIdentity);
            if (!config.Started)
                return Ok();

            config.Started = false;
            this.webInterviewConfigProvider.Store(questionnaireIdentity, config);

            return Ok();
        }
    }
}
