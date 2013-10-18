using System.IO;
using System.Web.Mvc;
using Main.Core.Services;

namespace Web.Supervisor.Controllers
{
    /// <summary>
    ///     Responsible for load images
    /// </summary>
    public class ResourceController : Controller
    {
        private readonly IFileStorageService fileStorageService;

        public ResourceController(IFileStorageService fileStorageService)
        {
            this.fileStorageService = fileStorageService;
        }

        [HttpGet]
        public ActionResult Images(string id)
        {
            Stream fileBytes = this.fileStorageService.RetrieveFile(id).Content;
            return this.File(fileBytes, "image/png");
        }

        [HttpGet]
        public ActionResult Thumb(string id)
        {
            Stream fileBytes = this.fileStorageService.RetrieveThumb(id).Content;
            return this.File(fileBytes, "image/png");
        }
    }
}