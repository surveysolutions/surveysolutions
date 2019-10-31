using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using CsvHelper;
using CsvHelper.Configuration;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Resources;
using WB.UI.Shared.Web.Attributes;
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
        private readonly IArchiveUtils archiveUtils;
        

        public WebInterviewSetupApiController(
            ICommandService commandService, 
            ILogger logger, 
            IWebInterviewConfigProvider webInterviewConfigProvider,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory, 
            IAssignmentsService assignmentsService,
            IInvitationService invitationService, 
            IPlainKeyValueStorage<EmailProviderSettings> emailProviderSettingsStorage,
            IArchiveUtils archiveUtils) : base(commandService, logger)
        {
            this.webInterviewConfigProvider = webInterviewConfigProvider;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.assignmentsService = assignmentsService;
            this.invitationService = invitationService;
            this.emailProviderSettingsStorage = emailProviderSettingsStorage;
            this.archiveUtils = archiveUtils;
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
            var status = this.invitationService.GetEmailDistributionStatus();

            var totalInvitationsCount = invitationService.GetCountOfInvitations(questionnaireIdentity);
            var notSentInvitationsCount = invitationService.GetCountOfNotSentInvitations(questionnaireIdentity);
            var sentInvitationsCount = invitationService.GetCountOfSentInvitations(questionnaireIdentity);

            return Ok(new
            {
                Title = questionnaire.Title,
                FullName = string.Format(Pages.QuestionnaireNameFormat, questionnaire.Title, questionnaire.Version),
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
        public IHttpActionResult EmailDistributionStatus()
        {
            InvitationDistributionStatus status = this.invitationService.GetEmailDistributionStatus();
            if (status == null)
                return NotFound();

            QuestionnaireBrowseItem questionnaire = this.FindQuestionnaire(status.QuestionnaireIdentity);
            if (questionnaire == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                Title = questionnaire.Title,
                FullName = string.Format(Pages.QuestionnaireNameFormat, questionnaire.Title, questionnaire.Version),
                QuestionnaireIdentity = new QuestionnaireIdentity(questionnaire.QuestionnaireId, questionnaire.Version),
                Status = status
            });
        }

        [HttpGet]
        public HttpResponseMessage ExportInvitationErrors()
        {
            InvitationDistributionStatus status = this.invitationService.GetEmailDistributionStatus();
            if (status == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            using (MemoryStream resultStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(resultStream))
            using (var csvWriter = new CsvWriter(streamWriter, new Configuration{Delimiter = "\t"}))
            {
                csvWriter.WriteHeader<InvitationSendError>();

                csvWriter.NextRecord();
                csvWriter.WriteRecords(status.Errors);
                csvWriter.Flush();
                streamWriter.Flush();

                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                resultStream.Seek(0, SeekOrigin.Begin);

                response.Content = new ByteArrayContent(archiveUtils.CompressStream(resultStream,"notSentInvitations.tab"));

                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = "notSentInvitations.zip"
                };

                return response;
            }
        }

        [HttpPost]
        public IHttpActionResult CancelEmailDistribution()
        {
            invitationService.CancelEmailDistribution();
            return Ok();
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
