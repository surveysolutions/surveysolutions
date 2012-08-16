using System;
using System.Web;
using System.Web.Mvc;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Vertical;
using RavenQuestionnaire.Core.Views.Questionnaire;
using RavenQuestionnaire.Core.Views.StatusReport;

namespace Web.Supervisor.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IViewRepository viewRepository;

        public DashboardController(IViewRepository viewRepository)
        {
            this.viewRepository = viewRepository;
        }


        public ActionResult Index()
        {
            StatusReportView model =
                viewRepository.Load<StatusReportViewInputModel, StatusReportView>(new StatusReportViewInputModel());
            return View(model);
        }

        public ActionResult Status(string questionnaireId, Guid statusId)
        {
            CQStatusReportView model =
                viewRepository.Load<CQStatusReportViewInputModel, CQStatusReportView>(
                    new CQStatusReportViewInputModel(questionnaireId, statusId));
            return View(model);
        }

        public ActionResult Questionnaires(QuestionnaireBrowseInputModel input)
        {
            QuestionnaireBrowseView model =
                viewRepository.Load<QuestionnaireBrowseInputModel, QuestionnaireBrowseView>(input);
            return View(model);
        }

        public ActionResult NewSurvey(string id)
        {
            Guid key;
            if (!Guid.TryParse(id, out key))
                throw new HttpException("404");
            Guid newSurveyPublicKey = Guid.NewGuid();
            var commandService = NcqrsEnvironment.Get<ICommandService>();
            commandService.Execute(new CreateCompleteQuestionnaireCommand(newSurveyPublicKey, key));
            return RedirectToAction("Survey", new {id = newSurveyPublicKey});
        }

        public ViewResult Survey(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new HttpException(404, "Invalid query string parameters");
            var input = new CompleteQuestionnaireViewInputModel(id) {};
            CompleteQuestionnaireViewV model =
                viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireViewV>(input);

            return View(model);
        }
    }
}