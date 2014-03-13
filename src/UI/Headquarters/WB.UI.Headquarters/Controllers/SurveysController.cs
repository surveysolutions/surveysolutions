using System;
using System.Web.Mvc;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Headquarters.Authentication;
using WB.Core.BoundedContexts.Headquarters.Commands.Survey;
using WB.Core.BoundedContexts.Headquarters.ViewFactories;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = ApplicationRoles.Headquarter)]
    public class SurveysController : Controller
    {
        private readonly ISurveyViewFactory surveyViewFactory;
        private readonly ICommandService commandService;

        public SurveysController(ISurveyViewFactory surveyViewFactory, ICommandService commandService)
        {
            this.surveyViewFactory = surveyViewFactory;
            this.commandService = commandService;
        }

        public ActionResult Index()
        {
            SurveyLineView[] surveys = this.surveyViewFactory.GetAllLineViews();

            return this.View(surveys);
        }

        public ActionResult StartNew()
        {
            return this.View(new NewSurveyModel());
        }

        [HttpPost]
        public ActionResult StartNew(NewSurveyModel model)
        {
            Guid newSurveyId = Guid.NewGuid();

            this.commandService.Execute(new StartNewSurvey(newSurveyId, model.Name));

            return this.RedirectToAction("Index");
        }

        public ActionResult RegisterSupervisorAccount(Guid? id)
        {
            return this.View(new SupervisorAccountModel());
        }

        [HttpPost]
        public ActionResult RegisterSupervisorAccount(Guid id, SupervisorAccountModel model)
        {
            if (ModelState.IsValid)
            {
                this.commandService.Execute(new RegisterSupervisorAccount(id) {
                   Login = model.Login,
                   Password = model.Password
                });

                return RedirectToAction("Index"); // todo ank: change when details action available.
            }

            return View(model);
        }

    }
}