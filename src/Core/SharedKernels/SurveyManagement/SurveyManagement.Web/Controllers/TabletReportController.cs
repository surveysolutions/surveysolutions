using System.Web.Mvc;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.TabletInformation;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
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