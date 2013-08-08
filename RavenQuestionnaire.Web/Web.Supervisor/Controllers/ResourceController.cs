using Main.Core.Services;

namespace Web.Supervisor.Controllers
{
    using System.Web.Mvc;

    /// <summary>
    /// Responsible for load images
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
            var fileBytes = this.fileStorageService.RetrieveFile(id).Content;
            return this.File(fileBytes, "image/png");
        }

        [HttpGet]
        public ActionResult Thumb(string id)
        {
            var fileBytes = this.fileStorageService.RetrieveThumb(id).Content;
            return this.File(fileBytes, "image/png");
        }
    }
}
