using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models.Reports;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.Controllers
{
    public class ReportsController : Controller
    {
        private readonly IMapReport mapReport;

        public ReportsController(IMapReport mapReport)
        {
            this.mapReport = mapReport;
        }

        [Authorize(Roles = "Administrator, Headquarter")]
        [ActivePage(MenuItem.SurveyAndStatuses)]
        public ActionResult SurveysAndStatuses()
        {
            var model = new SurveysAndStatusesModel();
            model.DataUrl = Url.RouteUrl(
                new
                {
                    controller = "ReportDataApi",
                    action = "HeadquarterSurveysAndStatusesReport"
                });

            model.ResponsiblesUrl = Url.RouteUrl(new
            {
                controller = "UsersTypeahead",
                action = "Supervisors"
            });

            model.ReportName = Pages.SurveysAndStatuses_Overview;
            model.Subtitle = Pages.SurveysAndStatuses_HeadquartersDescription;
            return this.View(model);
        }

        [Authorize(Roles = "Administrator, Supervisor, Headquarter")]
        [ActivePage(MenuItem.MapReport)]
        public ActionResult MapReport()
        {
            var questionnaires = this.mapReport.GetQuestionnaireIdentitiesWithGpsQuestions();

            return this.View(new
            {
                Questionnaires = questionnaires.GetQuestionnaireComboboxViewItems()
            });
        }

    }
}
