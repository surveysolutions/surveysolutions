using System;
using System.Web.Mvc;
using NLog;
using RavenQuestionnaire.Core.Services;

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
            Byte[] fileBytes = fileStorageService.RetrieveFile(id);
            return File(fileBytes, "image/png");
        }

    }
}
