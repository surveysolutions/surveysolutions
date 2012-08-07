using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Vertical;
using RavenQuestionnaire.Core.Views.Questionnaire;
using RavenQuestionnaire.Core.Views.StatusReport;
using RavenQuestionnaire.Core.Views.User;

namespace Web.Supervisor.Controllers
{
    public class DashboardController : Controller
    {
        private ICommandInvoker commandInvoker;
        private IViewRepository viewRepository;

        public DashboardController(ICommandInvoker commandInvoker, IViewRepository viewRepository)
        {
            this.commandInvoker = commandInvoker;
            this.viewRepository = viewRepository;
        }


        public ActionResult Index()
        {
            var model = viewRepository.Load<StatusReportViewInputModel, StatusReportView>(new StatusReportViewInputModel());
            return View(model);
        }

        public ActionResult Status(string questionnaireId, Guid statusId)
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
            var newSurveyPublicKey = Guid.NewGuid();
            var commandService = NcqrsEnvironment.Get<ICommandService>();
            commandService.Execute(new CreateCompleteQuestionnaireCommand(newSurveyPublicKey, key));
            return RedirectToAction("Survey", new { id = newSurveyPublicKey });
        }

        public ViewResult Survey(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new HttpException(404, "Invalid query string parameters");
            var input = new CompleteQuestionnaireViewInputModel(id) { };
            var model = viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireViewV>(input);

            return View(model);
        }

    }
}
