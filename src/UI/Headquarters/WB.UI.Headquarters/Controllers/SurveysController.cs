using System;
using System.Web;
using System.Web.Mvc;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Headquarters.Authentication;
using WB.Core.BoundedContexts.Headquarters.Commands.Survey;
using WB.Core.BoundedContexts.Headquarters.ViewFactories;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.Core.GenericSubdomains.Utils;
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

            return this.RedirectToAction("Details", new { id = newSurveyId.FormatGuid() });
        }

        public ActionResult Details(string id)
        {
            SurveyDetailsView survey = this.surveyViewFactory.GetDetailsView(id);

            if (survey == null)
                return new HttpNotFoundResult();

            return this.View(survey);
        }

        public ActionResult RegisterSupervisorAccount(Guid id)
        {
            ViewBag.SurveyId = id;
            return this.View(new SupervisorAccountModel());
        }

        [HttpPost]
        public ActionResult RegisterSupervisorAccount(Guid id, SupervisorAccountModel model)
        {
            if (ModelState.IsValid)
            {
                this.commandService.Execute(new RegisterSupervisorAccount(id, model.Login, model.Password));

                return RedirectToAction("Index"); // todo ank: change when details action available.
            }

            return View(model);
        }

    }
}