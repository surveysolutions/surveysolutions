using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Questionnaire.Api;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester.Controllers
{
    public class WebTesterController : Controller
    {
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly IEvictionObserver evictionService;
        private readonly ICommandService commandService;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IQuestionnaireImportService questionnaireImportService;

        public WebTesterController(
            IStatefulInterviewRepository statefulInterviewRepository,
            IEvictionObserver evictionService,
            ICommandService commandService,
            IQuestionnaireStorage questionnaireStorage,
            IQuestionnaireImportService questionnaireImportService)
        {
            this.statefulInterviewRepository = statefulInterviewRepository ?? throw new ArgumentNullException(nameof(statefulInterviewRepository));
            this.evictionService = evictionService;
            this.commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            this.questionnaireStorage = questionnaireStorage;
            this.questionnaireImportService = questionnaireImportService;
        }

        public ActionResult Run(Guid id) => this.View(new InterviewPageModel
        {
            Id = id.ToString()
        });

        public async Task<ActionResult> Redirect(Guid id)
        {
            QuestionnaireIdentity questionnaireIdentity;

            try
            {
                if (this.statefulInterviewRepository.Get(id.FormatGuid()) != null)
                {
                    evictionService.Evict(id);
                }

                questionnaireIdentity = await this.questionnaireImportService.ImportQuestionnaire(id);
            }
            catch (ApiException e) when (e.StatusCode == HttpStatusCode.PreconditionFailed)
            {
                return this.RedirectToAction("QuestionnaireWithErrors", "Error");
            }

            this.commandService.Execute(new CreateInterview(
                interviewId: id,
                userId: Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"),
                questionnaireId: questionnaireIdentity,
                answers: new List<InterviewAnswer>(),
                answersTime: DateTime.UtcNow,
                supervisorId: Guid.NewGuid(),
                interviewerId: Guid.NewGuid(),
                interviewKey: new InterviewKey(new Random().Next(99999999)),
                assignmentId: null));

            return this.Redirect($"~/WebTester/Interview/{id.FormatGuid()}/Cover");
        }

        public ActionResult Interview(string id)
        {
            try
            {
                var interviewPageModel = GetInterviewPageModel(id);
                if (interviewPageModel == null) 
                    throw new HttpException(404, string.Empty);

                return View(interviewPageModel);
            }
            catch (ApiException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                throw new HttpException(404, string.Empty);
            }
        }

        private InterviewPageModel GetInterviewPageModel(string id)
        {
            var interview = statefulInterviewRepository.Get(id);

            if (interview == null)
            {
                return null;
            }

            var questionnaire = this.questionnaireStorage.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            var interviewPageModel = new InterviewPageModel
            {
                Id = id,
                Title = $"{questionnaire.Title} | Web Tester",
                GoogleMapsKey = ConfigurationSource.Configuration["GoogleMapApiKey"]
            };
            return interviewPageModel;
        }

        public ActionResult Section(string id, string sectionId)
        {
            var interview = this.statefulInterviewRepository.Get(id);

            if (interview == null)
            {
                throw new HttpException(404, string.Empty);
            }

            var targetSectionIsEnabled = interview?.IsEnabled(Identity.Parse(sectionId));
            if (targetSectionIsEnabled != true)
            {
                var firstSectionId = interview.GetAllEnabledGroupsAndRosters().First().Identity.ToString();
                var uri = $@"~/WebTester/Interview/{interview.Id:N}/Section/{firstSectionId}";

                return Redirect(uri);
            }

            var model = GetInterviewPageModel(id);
            if (model == null)
            {
                throw new HttpException(404, string.Empty);
            }

            return this.View("Interview", model);
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
        public string Id { get; set; }
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
