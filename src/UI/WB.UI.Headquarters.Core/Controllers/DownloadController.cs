using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.UI.Headquarters.API;
using WB.UI.Headquarters.Code.Workspaces;
using WB.UI.Headquarters.Services;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Headquarters.Controllers
{
    [AllowPrimaryWorkspaceFallback]
    public class DownloadController : Controller
    {
        private readonly IQRCodeHelper qRCodeHelper;
        private readonly IClientApkProvider clientApkProvider;
        private readonly IVirtualPathService pathService;

        public DownloadController(IQRCodeHelper qRCodeHelper, 
            IClientApkProvider clientApkProvider, 
            IVirtualPathService pathService)
        {
            this.qRCodeHelper = qRCodeHelper;
            this.clientApkProvider = clientApkProvider;
            this.pathService = pathService;
        }

        public async Task<IActionResult> Index()
        {
            var smallApkUrl = Url.Action("GetLatestVersion", "DownloadApi");
            var fullApkUrl = Url.Action("GetLatestExtendedVersion", "DownloadApi");

            return View(new
            {
                SupportQRCodeGeneration = qRCodeHelper.SupportQRCodeGeneration(),
                SmallApkQRUrl = qRCodeHelper.GetQRCodeAsBase64StringSrc(pathService.GetAbsolutePath(smallApkUrl), 250, 250),
                FullApkQRUrl = qRCodeHelper.GetQRCodeAsBase64StringSrc(pathService.GetAbsolutePath(fullApkUrl), 250, 250),
                SmallApkUrl = smallApkUrl,
                FullApkUrl = fullApkUrl,
                SmallApkVersion = await clientApkProvider.GetApplicationVersionString(ClientApkInfo.InterviewerFileName),
                FullApkVersion = await clientApkProvider.GetApplicationVersionString(ClientApkInfo.InterviewerExtendedFileName),
            });
        }

        public async Task<IActionResult> Supervisor()
        {
            var apkUrl = Url.Action("GetLatestSupervisor", "DownloadApi");
            return View(new
            {
                SupportQRCodeGeneration = qRCodeHelper.SupportQRCodeGeneration(),
                ApkQRUrl = qRCodeHelper.GetQRCodeAsBase64StringSrc(pathService.GetAbsolutePath(apkUrl), 250, 250),
                ApkUrl = apkUrl,
                SupervisorVersion = await clientApkProvider.GetApplicationVersionString(ClientApkInfo.SupervisorFileName)
            });
        }
    }
}
