using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.EmailProviders;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Headquarters.Filters;

namespace WB.UI.Headquarters.Controllers.Api.WebInterview
{
    [Route("api/v1/webInterviewSettings")]
    [Authorize(Roles = "Administrator, Headquarter")]
    [ResponseCache(NoStore = true)]
    public class WebInterviewSettingsApiController : ControllerBase
    {
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IWebInterviewConfigProvider webInterviewConfigProvider;
        private readonly IEmailService emailService;
        private readonly ISendRemindersNowService sendRemindersNowService;
        private readonly IReminderEmailDeliveryReconciliationService deliveryReconciliationService;


        public WebInterviewSettingsApiController(
            IWebInterviewConfigProvider webInterviewConfigProvider,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IEmailService emailService,
            ISendRemindersNowService sendRemindersNowService,
            IReminderEmailDeliveryReconciliationService deliveryReconciliationService)
        {
            this.webInterviewConfigProvider = webInterviewConfigProvider;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.emailService = emailService;
            this.sendRemindersNowService = sendRemindersNowService;
            this.deliveryReconciliationService = deliveryReconciliationService;
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

        [ValidateAntiForgeryToken]
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

        [ValidateAntiForgeryToken]
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

        [ValidateAntiForgeryToken]
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
            public bool? AllowTranscriptDownloading { get;set; }        
        }

        [ValidateAntiForgeryToken]
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
            config.AllowTranscriptDownloading = updateModel.AllowTranscriptDownloading ?? true;            
            
            this.webInterviewConfigProvider.Store(questionnaireIdentity, config);

            return Ok();
        }

        [ValidateAntiForgeryToken]
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

        [ValidateAntiForgeryToken]
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

        public class SendRemindersNowModel
        {
            [Required] public EmailTextTemplateType? Type { get; set; }
            [Required] public System.Guid? RequestId { get; set; }
        }

        [ValidateAntiForgeryToken]
        [Route(@"{id}/sendReminders")]
        [HttpPost]
        public async Task<IActionResult> SendRemindersNow(string id, [FromBody] SendRemindersNowModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.RequestId == System.Guid.Empty)
                return BadRequest(new { error = "A request ID is required." });

            if (!QuestionnaireIdentity.TryParse(id, out var questionnaireIdentity))
                return NotFound();

            if (model.Type is not (EmailTextTemplateType.Reminder_NoResponse
                or EmailTextTemplateType.Reminder_PartialResponse
                or EmailTextTemplateType.RejectEmail))
                return BadRequest(new { error = "Only reminder and rejected-interview templates can be sent now." });

            var existingStatus = await sendRemindersNowService.GetStatusAsync(questionnaireIdentity, model.RequestId.Value)
                .ConfigureAwait(false);
            if (existingStatus != null)
            {
                if (existingStatus.Type != model.Type.Value)
                    return Conflict(new { error = "The request ID has already been used for a different reminder request." });

                return AcceptedAtAction(nameof(SendRemindersNowStatus),
                    new { id, requestId = existingStatus.RequestId }, existingStatus);
            }

            QuestionnaireBrowseItem questionnaire = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);
            if (questionnaire == null)
                return NotFound();

            var config = this.webInterviewConfigProvider.GetUncached(questionnaireIdentity);
            if (config == null || !config.Started)
                return BadRequest(new { error = "Web interview is not started." });

            if (!emailService.IsConfigured())
                return BadRequest(new { error = "Email sending is not configured." });

            SendRemindersNowStatus status;
            try
            {
                status = await sendRemindersNowService.SendAsync(questionnaireIdentity, questionnaire.Title,
                    model.Type.Value, model.RequestId.Value).ConfigureAwait(false);
            }
            catch (ReminderRequestIdConflictException exception)
            {
                return Conflict(new { error = exception.Message });
            }

            return AcceptedAtAction(nameof(SendRemindersNowStatus), new { id, requestId = status.RequestId }, status);
        }

        [Route(@"{id}/sendReminders/{requestId:guid}")]
        [HttpGet]
        public async Task<IActionResult> SendRemindersNowStatus(string id, System.Guid requestId)
        {
            if (!QuestionnaireIdentity.TryParse(id, out var questionnaireIdentity))
                return NotFound();

            var status = await sendRemindersNowService.GetStatusAsync(questionnaireIdentity, requestId)
                .ConfigureAwait(false);
            if (status == null)
                return NotFound();

            return Ok(status);
        }

        [Authorize(Roles = "Administrator")]
        [Route(@"{id}/unresolvedReminderDeliveries")]
        [HttpGet]
        public async Task<IActionResult> GetUnresolvedReminderDeliveries(string id, int offset = 0, int limit = 100)
        {
            if (offset < 0 || limit < 1 || limit > 100)
                return BadRequest();

            if (!QuestionnaireIdentity.TryParse(id, out var questionnaireIdentity))
                return NotFound();

            if (questionnaireBrowseViewFactory.GetById(questionnaireIdentity) == null)
                return NotFound();

            var result = await deliveryReconciliationService.GetUnresolvedAsync(questionnaireIdentity, offset, limit)
                .ConfigureAwait(false);
            return Ok(result);
        }

        public class ReconcileReminderDeliveryModel
        {
            [Required] public bool? Delivered { get; set; }
        }

        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        [Route(@"{id}/unresolvedReminderDeliveries/{invitationId:int}")]
        [HttpPost]
        public async Task<IActionResult> ReconcileUnresolvedReminderDelivery(string id, int invitationId,
            [FromBody] ReconcileReminderDeliveryModel model)
        {
            if (!ModelState.IsValid || model?.Delivered == null)
                return BadRequest(ModelState);

            if (!QuestionnaireIdentity.TryParse(id, out var questionnaireIdentity))
                return NotFound();

            if (questionnaireBrowseViewFactory.GetById(questionnaireIdentity) == null)
                return NotFound();

            try
            {
                if (!await deliveryReconciliationService.ReconcileAsync(questionnaireIdentity, invitationId,
                        model.Delivered.Value).ConfigureAwait(false))
                    return NotFound();
            }
            catch (ReminderDeliveryInProgressException exception)
            {
                return Conflict(new { error = exception.Message });
            }
            catch (ReminderDeliveryReconciliationInProgressException exception)
            {
                return Conflict(new { error = exception.Message });
            }

            return NoContent();
        }
    }
}
