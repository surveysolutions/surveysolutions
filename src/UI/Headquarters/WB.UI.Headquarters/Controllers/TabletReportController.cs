using System.Linq;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
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
        public ActionResult DownloadPackages(string fileName)
        {
            var hostName = this.Request?.Url?.Host.Split('.').FirstOrDefault() ?? @"unknownhost";
            return this.File(this.tabletInformationService.GetFullPathToContentFile(fileName), "application/zip", 
                this.tabletInformationService.GetFileName(fileName, hostName));
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult PackagesInfo()
        {
            return this.View();
        }
    }
}