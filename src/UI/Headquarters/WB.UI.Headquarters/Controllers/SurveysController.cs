using System;
using System.Web;
using System.Web.Mvc;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Headquarters.Authentication;
using WB.Core.BoundedContexts.Headquarters.Commands.Survey;
using WB.Core.BoundedContexts.Headquarters.Exceptions;
using WB.Core.BoundedContexts.Headquarters.ViewFactories;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.Core.GenericSubdomains.Utils;
using WB.UI.Headquarters.Models;
using WB.UI.Headquarters.Resources;

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

        public ActionResult RegisterSupervisor(string id)
        {
            SurveyDetailsView survey = this.surveyViewFactory.GetDetailsView(id);

            if (survey == null)
                return new HttpNotFoundResult();

            return this.View(new SupervisorModel
            {
                SurveyId = id,
                SurveyTitle = survey.Name
            });
        }

        [HttpPost]
        public ActionResult RegisterSupervisor(string id, SupervisorModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    this.commandService.Execute(new RegisterSupervisor(Guid.Parse(id), model.Login, model.Password));

                    return RedirectToAction("Details", new { id });
                }
                catch (FormatException)
                {
                    ModelState.AddModelError(string.Empty, SurveyResources.SurveyIdHasWrongFormat);
                }
                catch (SurveyException exception)
                {
                    ModelState.AddModelError(string.Empty, exception.Message);
                }
            }

            SurveyDetailsView survey = this.surveyViewFactory.GetDetailsView(id);

            if (survey == null)
                return new HttpNotFoundResult();

            model.SurveyId = id;
            model.SurveyTitle = survey.Name;
            return View(model);
        }

    }
}