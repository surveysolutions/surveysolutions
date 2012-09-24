using System;
using System.Web.Mvc;
using Main.Core.Services;
using NLog;

namespace Web.CAPI.Controllers
{
    public class ResourceController : Controller
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private IFileStorageService fileStorageService;

        public ResourceController(IFileStorageService fileStorageService)
        {
            this.fileStorageService = fileStorageService;
        }

        [HttpGet]
        public ActionResult Images(string id)
        {
            var fileBytes = fileStorageService.RetrieveFile(id).Content;
            return File(fileBytes, "image/png");
        }
        [HttpGet]
        public ActionResult Thumb(string id)
        {
            var fileBytes = fileStorageService.RetrieveThumb(id).Content;
            return File(fileBytes, "image/png");
        }
    }
}
