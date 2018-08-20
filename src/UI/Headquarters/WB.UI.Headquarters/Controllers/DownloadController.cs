using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
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
        public ActionResult Index()
        {
            var model = new DownloadModel();
            model.QRBase64Apk = qRCodeHelper.GetQRCodeAsBase64StringSrc(qRCodeHelper.GetFullUrl(Url.HttpRouteUrl("DefaultApiWithAction", new { action = "GetLatestVersion", controller = "InterviewerSync" })), 250, 250);
            model.QRBase64ApkWithMaps = qRCodeHelper.GetQRCodeAsBase64StringSrc(qRCodeHelper.GetFullUrl(Url.HttpRouteUrl("DefaultApiWithAction", new { action = "GetLatestExtendedVersion", controller = "InterviewerSync" })), 250, 250);

            return View(model);
        }

        public ActionResult Supervisor()
        {
            var model = new DownloadModel();
            model.QRBase64Apk = qRCodeHelper.GetQRCodeAsBase64StringSrc(qRCodeHelper.GetFullUrl(Url.HttpRouteUrl("DefaultApiWithAction", new { action = "GetLatestSupervisor", controller = "InterviewerSync" })), 250, 250);
            return View(model);
        }
    }
}
