using System.Web.Mvc;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [LimitsFilter]
    public class TabletReportController : Controller
    {
        private readonly ITabletInformationService tabletInformationService;

        public TabletReportController(ITabletInformationService tabletInformationService)
        {
            this.tabletInformationService = tabletInformationService;
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult Packages()
        {
            return this.View(this.tabletInformationService.GetAllTabletInformationPackages());
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult DownloadPackages(string fileName)
        {
            return this.File(this.tabletInformationService.GetFullPathToContentFile(fileName), "application/zip",
                this.tabletInformationService.GetPackageNameWithoutRegistrationId(fileName));
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult PackagesInfo()
        {
            return this.View();
        }
    }
}