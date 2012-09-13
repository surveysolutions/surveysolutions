using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Kaliko.ImageLibrary;
using Kaliko.ImageLibrary.Filters;
using NLog;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using RavenQuestionnaire.Core;
using Main.Core.Commands.File;
using Main.Core.Commands.Questionnaire;
using RavenQuestionnaire.Core.Services;
using Main.Core.Utility;

using LogManager = NLog.LogManager;

namespace RavenQuestionnaire.Web.Controllers
{
    using RavenQuestionnaire.Core.Views.Event.File;

    public class ViewDataUploadFilesResult
    {
        public string name { get; set; }
        public int size { get; set; }
        public string type { get; set; }
        public string url { get; set; }
        public string thumbnail_url { get; set; }
        public string title { get; set; }
        public string desc { get; set; }
    }
    public class FileBrowseItemClient
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
    public class ResourceController : Controller
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
       // private ICommandInvoker commandInvoker;
        private ICommandService commandService;
        private IViewRepository viewRepository;
        private IFileStorageService fileStorageService;

        public ResourceController(IFileStorageService fileStorageService/*, ICommandInvoker commandInvoker*/, IViewRepository viewRepository)
        {
           // this.commandInvoker = commandInvoker;
            this.viewRepository = viewRepository;
            this.fileStorageService = fileStorageService;
            this.commandService = NcqrsEnvironment.Get<ICommandService>();
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
        public ActionResult Index(FileBrowseInputModel input)
        {
            var model = viewRepository.Load<FileBrowseInputModel, FileBrowseView>(input);
            return View(model);
        }
        [HttpPost]
        public ActionResult UpdateDesc(FileBrowseItemClient meta)
        {
            try
            {
                var command = new UpdateFileMetaCommand(Guid.Parse(meta.Id), meta.Title, meta.Description);
                commandService.Execute(command);
                return Json(new { message = "saved" });
            }
            catch(Exception ex)
            {
                return Json(new { message = ex.Message });
            }
        }

        public ActionResult Upload()
        {
            return View();
        }

        //DONT USE THIS IF YOU NEED TO ALLOW LARGE FILES UPLOADS
        [HttpGet]
        public ActionResult Delete(string id)
        {
       //     var filename = id;

            try
            {

                commandService.Execute(new DeleteFileCommand(Guid.Parse(id)));
            }
            catch (Exception exception)
            {
                logger.Error("Can't delete image. "+exception.Message);
            }
            return RedirectToAction("Index", "Resource", new FileBrowseInputModel());
        }

        //DONT USE THIS IF YOU NEED TO ALLOW LARGE FILES UPLOADS
        [HttpPost]
        public ActionResult UploadFiles()
        {
            var r = new List<ViewDataUploadFilesResult>();

            foreach (string file in Request.Files)
            {
                var statuses = new List<ViewDataUploadFilesResult>();
                var headers = Request.Headers;

                if (string.IsNullOrEmpty(headers["X-File-Name"]))
                {
                    UploadWholeFile(Request, statuses);
                }
                else
                {
                    throw new HttpRequestValidationException("Attempt to upload too big file");
                }

                JsonResult result = Json(statuses);
                result.ContentType = "text/plain";

                return result;
            }

            return Json(r);
        }
       

        //DONT USE THIS IF YOU NEED TO ALLOW LARGE FILES UPLOADS
        private void UploadWholeFile(HttpRequestBase request, List<ViewDataUploadFilesResult> statuses)
        {
            for (int i = 0; i < request.Files.Count; i++)
            {
                var title = request["title"];
                var desc = request["desc"];
                var file = request.Files[i];
                var command = new UploadFileCommand(Guid.NewGuid(), title, desc,
                                                    file.InputStream);
           
                commandService.Execute(command);

                file.InputStream.Position = 0;
                var image = new KalikoImage(file.InputStream);
                int thumbWidth, thumbHeight;
                var thumbData = ResizeImage(image, 160, 120, out thumbWidth, out thumbHeight);
                var bytes = new byte[thumbData.Length];
                thumbData.Read(bytes, 0, (int)thumbData.Length);
                statuses.Add(new ViewDataUploadFilesResult
                                 {
                                     name = file.FileName,
                                     size = (int)thumbData.Length,
                                     type = file.ContentType,
                                     thumbnail_url = @"data:image/png;base64," + Convert.ToBase64String(bytes),
                                     title = title,
                                     desc = desc
                                 });
            }
        }

        private MemoryStream ResizeImage(KalikoImage image, int width, int height, out int newWidth, out int newHeight)
        {
            var thumb = image.GetThumbnailImage(width, height, ThumbnailMethod.Fit);
            thumb.ApplyFilter(new UnsharpMaskFilter(1.4, 0.32));

            var ms = new MemoryStream();
            thumb.SavePng(ms, 80);
            ms.Position = 0;

        //    var thumbData = new byte[ms.Length];
        //    ms.Read(thumbData, 0, thumbData.Length);

            newHeight = thumb.Height;
            newWidth = thumb.Width;

            return ms;
        }
    }
}
