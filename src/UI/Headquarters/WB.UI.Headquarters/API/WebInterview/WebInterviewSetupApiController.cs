using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Resources;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.API.WebInterview
{
    [Authorize(Roles = "Administrator, Headquarter")]
    [ApiNoCache]
    [CamelCase]
    public class WebInterviewSetupApiController : BaseApiController
    {
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IWebInterviewConfigProvider webInterviewConfigProvider;
        private readonly IAssignmentsService assignmentsService;
        private readonly IInvitationService invitationService;
        private readonly IPlainKeyValueStorage<EmailProviderSettings> emailProviderSettingsStorage;

        public WebInterviewSetupApiController(
            ICommandService commandService, 
            ILogger logger, 
            IWebInterviewConfigProvider webInterviewConfigProvider,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory, 
            IAssignmentsService assignmentsService,
            IInvitationService invitationService, 
            IPlainKeyValueStorage<EmailProviderSettings> emailProviderSettingsStorage) : base(commandService, logger)
        {
            this.webInterviewConfigProvider = webInterviewConfigProvider;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.assignmentsService = assignmentsService;
            this.invitationService = invitationService;
            this.emailProviderSettingsStorage = emailProviderSettingsStorage;
        }

        [HttpGet]
        public IHttpActionResult InvitationsInfo(string id)
        {
            if (!QuestionnaireIdentity.TryParse(id, out var questionnaireIdentity))
            {
                return NotFound();
            }

            QuestionnaireBrowseItem questionnaire = this.FindQuestionnaire(questionnaireIdentity);
            if (questionnaire == null)
            {
                return NotFound();
            }

            var config = this.webInterviewConfigProvider.Get(QuestionnaireIdentity.Parse(id));
            var emailProviderSettings = this.emailProviderSettingsStorage.GetById(AppSetting.EmailProviderSettings);

            var totalAssignmentsCount = assignmentsService.GetCountOfAssignments(questionnaireIdentity);
            var totalInvitationsCount = invitationService.GetCountOfInvitations(questionnaireIdentity);
            var notSentInvitationsCount = invitationService.GetCountOfNotSentInvitations(questionnaireIdentity);

            return Ok(new
            {
                Title = questionnaire.Title,
                FullName = string.Format(Pages.QuestionnaireNameFormat, questionnaire.Title, questionnaire.Version),
                QuestionnaireIdentity = new QuestionnaireIdentity(questionnaire.QuestionnaireId, questionnaire.Version),
                Started = config.Started,
                TotalAssignmentsCount = totalAssignmentsCount,
                TotalInvitationsCount = totalInvitationsCount,
                NotSentInvitationsCount = notSentInvitationsCount,
                EmailProvider = emailProviderSettings?.Provider ?? EmailProvider.None
            });
        }

        private QuestionnaireBrowseItem FindQuestionnaire(string id)
        {
            return !QuestionnaireIdentity.TryParse(id, out var questionnaireIdentity) ? null : FindQuestionnaire(questionnaireIdentity);
        }
        private QuestionnaireBrowseItem FindQuestionnaire(QuestionnaireIdentity questionnaireIdentity)
        {
            QuestionnaireBrowseItem questionnaire = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);
            return questionnaire;
        }
    }
}
