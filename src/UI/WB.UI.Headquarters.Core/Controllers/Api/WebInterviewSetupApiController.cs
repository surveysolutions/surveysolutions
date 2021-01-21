using System.Collections;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Headquarters.Controllers.Services;

namespace WB.UI.Headquarters.Controllers.Api
{
    [Authorize(Roles = "Administrator, Headquarter")]
    [ApiNoCache]
    [ResponseCache(NoStore = true)]
    [Route("api/{controller}/{action}/{id?}")]
    public class WebInterviewSetupApiController : ControllerBase
    {
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IWebInterviewConfigProvider webInterviewConfigProvider;
        private readonly IInvitationService invitationService;
        private readonly IPlainKeyValueStorage<EmailProviderSettings> emailProviderSettingsStorage;
        private readonly IArchiveUtils archiveUtils;
        

        public WebInterviewSetupApiController(IWebInterviewConfigProvider webInterviewConfigProvider,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IInvitationService invitationService, 
            IPlainKeyValueStorage<EmailProviderSettings> emailProviderSettingsStorage,
            IArchiveUtils archiveUtils) 
        {
            this.webInterviewConfigProvider = webInterviewConfigProvider;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.invitationService = invitationService;
            this.emailProviderSettingsStorage = emailProviderSettingsStorage;
            this.archiveUtils = archiveUtils;
        }

        [HttpGet]
        public IActionResult InvitationsInfo(string id)
        {
            if (!QuestionnaireIdentity.TryParse(id, out var questionnaireIdentity))
            {
                return NotFound();
            }

            QuestionnaireBrowseItem questionnaire = this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);
            if (questionnaire == null)
            {
                return NotFound();
            }

            var config = this.webInterviewConfigProvider.Get(QuestionnaireIdentity.Parse(id));
            var emailProviderSettings = this.emailProviderSettingsStorage.GetById(AppSetting.EmailProviderSettings);
            var status = this.invitationService.GetEmailDistributionStatus();

            var totalInvitationsCount = invitationService.GetCountOfInvitations(questionnaireIdentity);
            var notSentInvitationsCount = invitationService.GetCountOfNotSentInvitations(questionnaireIdentity);
            var sentInvitationsCount = invitationService.GetCountOfSentInvitations(questionnaireIdentity);

            return Ok(new
            {
                Title = questionnaire.Title,
                Version = questionnaire.Version,
                QuestionnaireIdentity = new QuestionnaireIdentity(questionnaire.QuestionnaireId, questionnaire.Version),
                Started = config.Started,
                TotalInvitationsCount = totalInvitationsCount,
                NotSentInvitationsCount = notSentInvitationsCount,
                SentInvitationsCount = sentInvitationsCount,
                EmailProvider = emailProviderSettings?.Provider ?? EmailProvider.None,
                Status = status
            });
        }

        [HttpGet]
        public IActionResult EmailDistributionStatus()
        {
            InvitationDistributionStatus status = this.invitationService.GetEmailDistributionStatus();
            if (status == null)
                return NotFound();

            QuestionnaireBrowseItem questionnaire = this.questionnaireBrowseViewFactory.GetById(status.QuestionnaireIdentity);
            if (questionnaire == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                Title = questionnaire.Title,
                Version = questionnaire.Version,
                QuestionnaireIdentity = new QuestionnaireIdentity(questionnaire.QuestionnaireId, questionnaire.Version),
                Status = status
            });
        }

        [HttpGet]
        public IActionResult ExportInvitationErrors()
        {
            InvitationDistributionStatus status = this.invitationService.GetEmailDistributionStatus();
            if (status == null)
                return NotFound();

            using MemoryStream resultStream = new MemoryStream();
            using var streamWriter = new StreamWriter(resultStream);
            using var csvWriter = new CsvWriter(streamWriter, new CsvConfiguration(CultureInfo.InvariantCulture) {Delimiter = "\t"});
            csvWriter.WriteHeader<InvitationSendError>();

            csvWriter.NextRecord();
            csvWriter.WriteRecords((IEnumerable) status.Errors);
            csvWriter.Flush();
            streamWriter.Flush();

            resultStream.Seek(0, SeekOrigin.Begin);
            return File(archiveUtils.CompressStream(resultStream, "notSentInvitations.tab"),
                "application/octet-stream",
                "notSentInvitations.zip",
                null, null);
        }

        [HttpPost]
        public IActionResult CancelEmailDistribution()
        {
            invitationService.CancelEmailDistribution();
            return Ok();
        }
    }
}
