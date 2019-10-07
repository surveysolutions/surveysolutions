using System.ComponentModel.DataAnnotations;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Controllers;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.API.WebInterview
{
    [RoutePrefix("api/v1/webInterviewSettings")]
    [Authorize(Roles = "Administrator, Headquarter")]
    [ApiNoCache]
    [CamelCase]
    public class WebInterviewSettingsApiController : BaseApiController
    {
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IWebInterviewConfigProvider webInterviewConfigProvider;


        public WebInterviewSettingsApiController(
            ICommandService commandService, 
            ILogger logger, 
            IWebInterviewConfigProvider webInterviewConfigProvider,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory) : base(commandService, logger)
        {
            this.webInterviewConfigProvider = webInterviewConfigProvider;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
        }

        public class UpdatePageTemplateModel
        {
            [Required] public WebInterviewUserMessages TitleType { get; set; }
            [Required] public string TitleText { get; set; }
            [Required] public WebInterviewUserMessages MessageType { get; set; }
            [Required] public string MessageText { get; set; }
        }

        [Route(@"{id}/pageTemplate")]
        [HttpPost]
        public IHttpActionResult UpdatePageTemplate(string id, [FromBody]UpdatePageTemplateModel updateModel)
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
            this.webInterviewConfigProvider.Store(questionnaireIdentity, config);

            return Ok();
        }

        public class UpdateEmailTemplateModel
        {
            [Required] public EmailTextTemplateType Type { get; set; }
            [Required] public string Subject { get; set; }
            [Required] public string Message { get; set; }
            [Required] public string PasswordDescription { get; set; }
            [Required] public string LinkText { get; set; }
        }

        [Route(@"{id}/emailTemplate")]
        [HttpPost]
        public IHttpActionResult UpdateEmailTemplate(string id, [FromBody]UpdateEmailTemplateModel updateModel)
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
        public IHttpActionResult UpdatePageMessage(string id, [FromBody]UpdatePageMessageModel updateModel)
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
        }

        [Route(@"{id}/additionalSettings")]
        [HttpPost]
        public IHttpActionResult UpdateAdditionalSettings(string id, [FromBody]UpdateAdditionalSettingsModel updateModel)
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

            this.webInterviewConfigProvider.Store(questionnaireIdentity, config);

            return Ok();
        }

        [Route(@"{id}/start")]
        [HttpPost]
        public IHttpActionResult Start(string id)
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
            config.BaseUrl = this.Url.Content("~/");
            this.webInterviewConfigProvider.Store(questionnaireIdentity, config);

            return Ok();
        }

        [Route(@"{id}/stop")]
        [HttpPost]
        public IHttpActionResult Stop(string id)
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
