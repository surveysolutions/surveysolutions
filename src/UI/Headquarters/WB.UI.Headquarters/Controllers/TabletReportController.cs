using System.IO;
using System.Linq;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.UI.Headquarters.Code;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [LimitsFilter]
    public class TabletReportController : Controller
    {
        private readonly ITabletInformationService tabletInformationService;
        private readonly IAuthorizedUser authorizedUser;
        private readonly IUserViewFactory userViewFactory;

        public TabletReportController(ITabletInformationService tabletInformationService,
            IAuthorizedUser authorizedUser, IUserViewFactory userViewFactory)
        {
            this.tabletInformationService = tabletInformationService;
            this.authorizedUser = authorizedUser;
            this.userViewFactory = userViewFactory;
        }

        [AuthorizeOr403(Roles = "Administrator")]
        public ActionResult DownloadPackages(string fileName)
        {
            var hostName = this.Request?.Url?.Host.Split('.').FirstOrDefault() ?? @"unknownhost";
            return this.File(this.tabletInformationService.GetFullPathToContentFile(fileName), "application/zip", 
                this.tabletInformationService.GetFileName(fileName, hostName));
        }

        [AuthorizeOr403(Roles = "Administrator")]
        public ActionResult PackagesInfo()
        {
            return this.View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public ActionResult UploadBackup()
        {
            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];

                if (file != null && file.ContentLength > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        file.InputStream.CopyTo(ms);

                        this.tabletInformationService.SaveTabletInformation(
                            content: ms.ToArray(),
                            androidId: @"manual-restore",
                            user: this.userViewFactory.GetUser(new UserViewInputModel(this.authorizedUser.Id)));
                    }
                }
            }
            
            return this.RedirectToAction("PackagesInfo");
        }
    }
}
