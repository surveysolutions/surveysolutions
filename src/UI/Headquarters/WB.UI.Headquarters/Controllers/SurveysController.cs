using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Authentication;
using WB.Core.BoundedContexts.Headquarters.ViewFactories;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = ApplicationRoles.Headquarter)]
    public class SurveysController : Controller
    {
        private readonly ISurveyViewFactory surveyViewFactory;

        public SurveysController(ISurveyViewFactory surveyViewFactory)
        {
            this.surveyViewFactory = surveyViewFactory;
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
            return this.RedirectToAction("Index");
        }
    }
}