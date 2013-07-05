using Main.Core.View;
using Main.Core.View.Questionnaire;
using WB.Core.GenericSubdomains.Logging;

namespace Web.Supervisor.Controllers
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    using Ncqrs.Commanding.ServiceModel;
    using Main.Core.Commands.Questionnaire.Completed;

    using Questionnaire.Core.Web.Helpers;
    using Web.Supervisor.Models;

    [Authorize(Roles = "Headquarter")]
    public class DashboardController : BaseController
    {
        private readonly IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireBrowseView> viewFactory;

        public DashboardController(ICommandService commandService, IGlobalInfoProvider globalProvider, ILog logger, IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireBrowseView> viewFactory)
            : base(commandService, globalProvider, logger)
        {
            this.viewFactory = viewFactory;
        }

        public ActionResult Questionnaires(QuestionnaireBrowseInputModel input)
        {
            ViewBag.ActivePage = MenuItem.Administration;
             var model = this.viewFactory.Load(input);
             return this.View(model);
        }
      
        public ActionResult NewSurvey(string id)
        {
            Guid key;
            if (!Guid.TryParse(id, out key))
                throw new HttpException("404");
            var newQuestionnairePublicKey = Guid.NewGuid();
            this.CommandService.Execute(new CreateCompleteQuestionnaireCommand(newQuestionnairePublicKey, key, this.GlobalInfo.GetCurrentUser()));
            return this.RedirectToAction("Assign", "HQ", new { Id = newQuestionnairePublicKey, Template = id });
        }
    }
}
