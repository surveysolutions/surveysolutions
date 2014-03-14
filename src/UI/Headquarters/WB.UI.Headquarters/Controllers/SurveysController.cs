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

        public ActionResult RegisterSupervisorAccount(string id)
        {
            SurveyDetailsView survey = this.surveyViewFactory.GetDetailsView(id);

            return this.View(new SupervisorAccountModel()
            {
                SurveyId = id,
                SurveyTitle = survey.Name
            });
        }

        [HttpPost]
        public ActionResult RegisterSupervisorAccount(string id, SupervisorAccountModel model)
        {
            if (ModelState.IsValid)
            {
                this.commandService.Execute(new RegisterSupervisorAccount(Guid.Parse(id), model.Login, model.Password));

                return RedirectToAction("Details", new { id});
            }

            SurveyDetailsView survey = this.surveyViewFactory.GetDetailsView(id);

            model.SurveyId = id;
            model.SurveyTitle = survey.Name;
            return View(model);
        }

    }
}