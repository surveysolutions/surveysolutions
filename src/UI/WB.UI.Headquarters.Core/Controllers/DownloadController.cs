using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.Controllers
{
    public class DownloadController : Controller
    {
        public DownloadController(IQRCodeHelper qRCodeHelper)
        {
            this.qRCodeHelper = qRCodeHelper;
        }

        private IQRCodeHelper qRCodeHelper;

        // GET: Download
        public IActionResult Index()
        {
            var model = new DownloadModel();
            var noMapsApkUrl = Url.RouteUrl(new { action = "GetLatestVersion", controller = "InterviewerSync" });
            var mapsApkUrl = Url.RouteUrl(new { action = "GetLatestExtendedVersion", controller = "InterviewerSync" });

            model.QRBase64Apk = qRCodeHelper.GetQRCodeAsBase64StringSrc(qRCodeHelper.GetFullUrl(noMapsApkUrl), 250, 250);
            model.QRBase64ApkWithMaps = qRCodeHelper.GetQRCodeAsBase64StringSrc(qRCodeHelper.GetFullUrl(mapsApkUrl), 250, 250);

            return View(model);
        }

        public IActionResult Supervisor()
        {
            var model = new DownloadModel();
            var httpRouteUrl = Url.RouteUrl(new { action = "GetLatestSupervisor", controller = "InterviewerSync" });
            model.QRBase64Apk = qRCodeHelper.GetQRCodeAsBase64StringSrc(qRCodeHelper.GetFullUrl(httpRouteUrl), 250, 250);
            return View(model);
        }
    }
}
