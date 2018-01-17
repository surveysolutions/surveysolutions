using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Questionnaire.Api;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Enumerator.Native.Questionnaire;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester.Controllers
{
    public class WebTesterController : Controller
    {
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly ICommandService commandService;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IDesignerWebTesterApi webTesterApi;
        private readonly IAppdomainsPerInterviewManager appdomainsPerInterviewManager;
        private readonly ITranslationManagementService translationManagementService;

        public static readonly Dictionary<Guid, QuestionnaireIdentity> Questionnaires = new Dictionary<Guid, QuestionnaireIdentity>();

        public WebTesterController(IStatefulInterviewRepository statefulInterviewRepository,
            ICommandService commandService,
            IQuestionnaireStorage questionnaireStorage,
            IDesignerWebTesterApi webTesterApi,
            IAppdomainsPerInterviewManager appdomainsPerInterviewManager, 
            ITranslationManagementService translationManagementService)
        {
            this.statefulInterviewRepository = statefulInterviewRepository ?? throw new ArgumentNullException(nameof(statefulInterviewRepository));
            this.commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            this.appdomainsPerInterviewManager = appdomainsPerInterviewManager ?? throw new ArgumentNullException(nameof(appdomainsPerInterviewManager));
            this.translationManagementService = translationManagementService;
            this.translationManagementService = translationManagementService ?? throw new ArgumentNullException(nameof(translationManagementService));
            this.questionnaireStorage = questionnaireStorage ?? throw new ArgumentNullException(nameof(questionnaireStorage));
            this.webTesterApi = webTesterApi ?? throw new ArgumentNullException(nameof(webTesterApi));
            this.appdomainsPerInterviewManager = appdomainsPerInterviewManager;
        }

        public async Task<ActionResult> Run(Guid id)
        {
            var questionnaire = await webTesterApi.GetQuestionnaireAsync(id.ToString());

            var questionnaireIdentity = new QuestionnaireIdentity(questionnaire.Document.PublicKey, 1);

            var translations = await webTesterApi.GetTranslationsAsync(id.ToString());

            var attachments = new List<QuestionnaireAttachment>();
            foreach (Attachment documentAttachment in questionnaire.Document.Attachments)
            {
                var content = await webTesterApi.GetAttachmentContentAsync(id.ToString(), documentAttachment.ContentId);
                attachments.Add(new QuestionnaireAttachment
                {
                    Id = documentAttachment.AttachmentId,
                    Content = content
                });

            }

            this.appdomainsPerInterviewManager.SetupForInterview(id, questionnaire.Document, questionnaire.Assembly);
            this.questionnaireStorage.StoreQuestionnaire(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, questionnaire.Document);
            
            this.translationManagementService.Delete(questionnaireIdentity);
            this.translationManagementService.Store(translations.Select(x => new TranslationInstance
            {
                QuestionnaireId = questionnaireIdentity,
                Value = x.Value,
                QuestionnaireEntityId = x.QuestionnaireEntityId,
                Type = x.Type,
                TranslationIndex = x.TranslationIndex,
                TranslationId = x.TranslationId
            }));

            this.commandService.Execute(new CreateInterview(
                interviewId: id,
                userId: Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"),
                questionnaireId: questionnaireIdentity,
                answers: new List<InterviewAnswer>(),
                answersTime: DateTime.UtcNow,
                supervisorId: Guid.NewGuid(),
                interviewerId: Guid.NewGuid(),
                interviewKey: new InterviewKey(00_00_00),
                assignmentId: null));

            return Redirect($"~/WebTester/Interview/{id.FormatGuid()}/Cover");
        }

        public async Task<ActionResult> Interview(string id)
        {
            await this.webTesterApi.GetQuestionnaireInfoAsync(Guid.Parse(id).ToString());
            var interview = statefulInterviewRepository.Get(id);

            var questionnaire = this.questionnaireStorage.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            return View(new InterviewPageModel
            {
                Title = $"{questionnaire.Title} | Web Tester",
                GoogleMapsKey = ConfigurationSource.Configuration["GoogleMapApiKey"]
            });
        }
    }

    public class QuestionnaireAttachment
    {
        public Guid Id { get; set; }
        public AttachmentContent Content { get; set; }
    }

    public class InterviewPageModel
    {
        public string Title { get; set; }
        public string GoogleMapsKey { get; set; }
    }

    public class ApiTestModel
    {
        public Guid Id { get; set; }
        public DateTime LastUpdated { get; set; }
        public int NumOfTranslations { get; set; }
        public List<string> Attaches { get; set; }
        public string Title { get; set; }
    }
}
