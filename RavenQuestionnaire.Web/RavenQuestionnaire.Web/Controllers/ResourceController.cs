using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Services;

namespace RavenQuestionnaire.Web.Controllers
{
    public class ResourceController : Controller
    {
        private ICommandInvoker commandInvoker;
        private IViewRepository viewRepository;
        private IFileStorageService fileStorageService;

        public ResourceController(IFileStorageService fileStorageService, ICommandInvoker commandInvoker, IViewRepository viewRepository)
        {
            this.commandInvoker = commandInvoker;
            this.viewRepository = viewRepository;
            this.fileStorageService = fileStorageService;
        }

        public ActionResult Images(string id)
        {
            Byte[] fileBytes = fileStorageService.RetrieveFile(id);
            return File(fileBytes, "image/png");   
        }
    }
}
