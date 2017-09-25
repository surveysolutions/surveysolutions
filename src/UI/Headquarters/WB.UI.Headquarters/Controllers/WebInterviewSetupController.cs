using System.Linq;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Services.WebInterview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models.WebInterview;

namespace WB.UI.Headquarters.Controllers
{
    [LimitsFilter]
    [Authorize(Roles = "Administrator, Headquarter")]
    [ObserverNotAllowed]
    [WebInterviewFeatureEnabled]
    public class WebInterviewSetupController : BaseController
    {
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IWebInterviewConfigurator configurator;
        private readonly IWebInterviewConfigProvider webInterviewConfigProvider;
        private readonly IPlainStorageAccessor<Assignment> assignments;
        private readonly IAssignmentsService assignmentsService;
        private readonly IWebInterviewNotificationService webInterviewNotificationService;

        // GET: WebInterviewSetup
        public WebInterviewSetupController(ICommandService commandService,
            ILogger logger, 
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IWebInterviewConfigurator configurator,
            IWebInterviewConfigProvider webInterviewConfigProvider,
            IPlainStorageAccessor<Assignment> assignments,
            IAssignmentsService assignmentsService,
            IWebInterviewNotificationService webInterviewNotificationService)
            : base(commandService, 
                  logger)
        {
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.configurator = configurator;
            this.webInterviewConfigProvider = webInterviewConfigProvider;
            this.assignments = assignments;
            this.assignmentsService = assignmentsService;
            this.webInterviewNotificationService = webInterviewNotificationService;
        }

        public ActionResult Start(string id)
        {
            this.ViewBag.ActivePage = MenuItem.Questionnaires;

            var config = this.webInterviewConfigProvider.Get(QuestionnaireIdentity.Parse(id));
            if (config.Started) return RedirectToAction("Started", new {id = id});

            QuestionnaireBrowseItem questionnaire = this.FindQuestionnaire(id);
            if (questionnaire == null)
            {
                return this.HttpNotFound();
            }
            
            var model = new SetupModel();
            model.QuestionnaireTitle = questionnaire.Title;
            model.QuestionnaireIdentity = new QuestionnaireIdentity(questionnaire.QuestionnaireId, questionnaire.Version);
            model.UseCaptcha = true;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Start")]
        public ActionResult StartPost(string id, SetupModel model)
        {
            this.ViewBag.ActivePage = MenuItem.Questionnaires;
            QuestionnaireBrowseItem questionnaire = this.FindQuestionnaire(id);
            if (questionnaire == null)
            {
                return this.HttpNotFound();
            }

            model.QuestionnaireTitle = questionnaire.Title;
            var questionnaireIdentity = QuestionnaireIdentity.Parse(id);

            this.configurator.Start(questionnaireIdentity, model.UseCaptcha);
            return this.RedirectToAction("Started", new { id = questionnaireIdentity.ToString() });
        }

        public ActionResult Started(string id)
        {
            this.ViewBag.ActivePage = MenuItem.Questionnaires;
            QuestionnaireBrowseItem questionnaire = this.FindQuestionnaire(id);
            if (questionnaire == null)
            {
                return this.HttpNotFound();
            }

            var questionnaireIdentity = QuestionnaireIdentity.Parse(id);

            var model = new SetupModel
            {
                QuestionnaireTitle = questionnaire.Title,
                QuestionnaireIdentity = questionnaireIdentity
            };

            model.AssignmentsCount =
                this.assignmentsService.GetCountOfAssignmentsReadyForWebInterview(questionnaireIdentity);

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Started")]
        public ActionResult StartedPost(string id)
        {
            var questionnaireId = QuestionnaireIdentity.Parse(id);
            this.configurator.Stop(questionnaireId);
            this.webInterviewNotificationService.ReloadInterviewByQuestionnaire(questionnaireId);

            return RedirectToAction("Index", "SurveySetup");
        }

        private QuestionnaireBrowseItem FindQuestionnaire(string id)
        {
            QuestionnaireIdentity questionnarieId;
            if (!QuestionnaireIdentity.TryParse(id, out questionnarieId))
            {
                return null;
            }

            QuestionnaireBrowseItem questionnaire = this.questionnaireBrowseViewFactory.GetById(questionnarieId);
            return questionnaire;
        }
    }
}