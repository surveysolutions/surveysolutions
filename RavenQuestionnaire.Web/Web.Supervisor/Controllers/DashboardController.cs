using Ncqrs;
using System;
using System.Web;
using System.Web.Mvc;
using RavenQuestionnaire.Core;
using Ncqrs.Commanding.ServiceModel;
using RavenQuestionnaire.Core.Views.StatusReport;
using RavenQuestionnaire.Core.Views.Questionnaire;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Views.Survey;


namespace Web.Supervisor.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private IViewRepository viewRepository;

        public DashboardController(IViewRepository viewRepository)
        {
            this.viewRepository = viewRepository;
        }
        
        public ActionResult Index()
        {
            var model = viewRepository.Load<StatusReportViewInputModel, StatusReportView>(new StatusReportViewInputModel());
            return View(model);
        }

        public ActionResult Status(Guid questionnaireId, Guid statusId)
        {
            var model = viewRepository.Load<CQStatusReportViewInputModel, CQStatusReportView>(new CQStatusReportViewInputModel(questionnaireId, statusId));
            return View(model);
        }

        public ActionResult Questionnaires(QuestionnaireBrowseInputModel input)
        {
            var model = viewRepository.Load<QuestionnaireBrowseInputModel, QuestionnaireBrowseView>(input);
            return View(model);
        }

        public ActionResult NewSurvey(string id)
        {
            Guid key;
            if (!Guid.TryParse(id, out key))
                throw new HttpException("404");
            var newQuestionnairePublicKey = Guid.NewGuid();
            var commandService = NcqrsEnvironment.Get<ICommandService>();
            commandService.Execute(new CreateCompleteQuestionnaireCommand(newQuestionnairePublicKey, key));
            return RedirectToAction("Assign", "Survey", new { Id = newQuestionnairePublicKey, Template = id });
        }
    }
}
