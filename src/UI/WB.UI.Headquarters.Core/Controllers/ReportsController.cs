using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models.Reports;
using WB.UI.Headquarters.Services;

namespace WB.UI.Headquarters.Controllers
{
    public class ReportsController : Controller
    {
        [Authorize(Roles = "Administrator, Headquarter")]
        [ActivePage(MenuItem.SurveyAndStatuses)]
        public ActionResult SurveysAndStatuses(SurveysAndStatusesModel model)
        {
            return this.View(model);
        }
    }
}
