using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Kaliko.ImageLibrary;
using Kaliko.ImageLibrary.Filters;
using Questionnaire.Core.Web.Helpers;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands.File;
using RavenQuestionnaire.Core.Commands.Questionnaire;
using RavenQuestionnaire.Core.Services;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.File;

namespace RavenQuestionnaire.Web.Controllers
{
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
        private ICommandInvoker commandInvoker;
        private IViewRepository viewRepository;
        private IFileStorageService fileStorageService;

        public ResourceController(IFileStorageService fileStorageService, ICommandInvoker commandInvoker, IViewRepository viewRepository)
        {
            this.commandInvoker = commandInvoker;
            this.viewRepository = viewRepository;
            this.fileStorageService = fileStorageService;
        }

        [HttpGet]
        public ActionResult Images(string id)
        {
            Byte[] fileBytes = fileStorageService.RetrieveFile(id);
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
                var command = new UpdateFileMetaCommand(meta.Id, meta.Title, meta.Description, GlobalInfo.GetCurrentUser());
                commandInvoker.Execute(command);
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
        public void Delete(string id)
        {
            var filename = id;
            var filePath = Path.Combine(Server.MapPath("~/Files"), filename);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
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

                var image = new KalikoImage(file.InputStream);
                int thumbWidth, thumbHeight, origWidth, origHeight;
                var thumbData = ResizeImage(image, 160, 120, out thumbWidth, out thumbHeight);
                var origData = ResizeImage(image, 1024, 768, out origWidth, out origHeight);

                var command = new UploadFileCommand(title, desc,
                                                    thumbData, thumbWidth,
                                                    thumbHeight,
                                                    origData, origWidth, origHeight,
                                                    GlobalInfo.GetCurrentUser());
                commandInvoker.Execute(command);

                statuses.Add(new ViewDataUploadFilesResult
                                 {
                                     name = file.FileName,
                                     size = origData.Length,
                                     type = file.ContentType,
                                     thumbnail_url = @"data:image/png;base64," + Convert.ToBase64String(thumbData),
                                     title = title,
                                     desc = desc
                                 });
            }
        }

        private byte[] ResizeImage(KalikoImage image, int width, int height, out int newWidth, out int newHeight)
        {
            var thumb = image.GetThumbnailImage(width, height, ThumbnailMethod.Fit);
            thumb.ApplyFilter(new UnsharpMaskFilter(1.4, 0.32));

            var ms = new MemoryStream();
            thumb.SavePng(ms, 80);
            ms.Position = 0;

            var thumbData = new byte[ms.Length];
            ms.Read(thumbData, 0, thumbData.Length);

            newHeight = thumb.Height;
            newWidth = thumb.Width;

            return thumbData;
        }
    }
}
