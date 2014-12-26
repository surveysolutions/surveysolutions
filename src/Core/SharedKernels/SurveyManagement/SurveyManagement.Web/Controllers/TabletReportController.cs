using System.Web.Mvc;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.SharedKernels.SurveyManagement.Services;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    public class TabletReportController : Controller
    {
        private readonly ITabletInformationService tabletInformationService;

        public TabletReportController(ILogger logger, ITabletInformationService tabletInformationService)
        {
            this.tabletInformationService = tabletInformationService;
        }

        [Authorize(Roles = "Headquarter, Supervisor")]
        public ActionResult Packages()
        {
            return this.View(this.tabletInformationService.GetAllTabletInformationPackages());
        }

        [Authorize(Roles = "Headquarter, Supervisor")]
        public ActionResult DownloadPackages(string fileName)
        {
            return this.File(this.tabletInformationService.GetFullPathToContentFile(fileName), "application/zip", fileName);
        }
    }
}