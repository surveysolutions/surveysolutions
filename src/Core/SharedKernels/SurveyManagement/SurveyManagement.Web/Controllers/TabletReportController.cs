using System.Web.Mvc;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.TabletInformation;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [LimitsFilter]
    public class TabletReportController : Controller
    {
        private readonly ITabletInformationService tabletInformationService;

        public TabletReportController(ILogger logger, ITabletInformationService tabletInformationService)
        {
            this.tabletInformationService = tabletInformationService;
        }

        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public ActionResult Packages()
        {
            return this.View(this.tabletInformationService.GetAllTabletInformationPackages());
        }

        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public ActionResult DownloadPackages(string fileName)
        {
            return this.File(this.tabletInformationService.GetFullPathToContentFile(fileName), "application/zip", fileName);
        }

        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public ActionResult Device(string id)
        {
            TabletLogView model = this.tabletInformationService.GetTabletLog(id);
            return this.View(model);
        }
    }
}