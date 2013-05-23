// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResourceController.cs" company="World bank">
//   2012
// </copyright>
// <summary>
//   The view data upload files result.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using WB.Common;

namespace RavenQuestionnaire.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Web;
    using System.Web.Mvc;

    using Kaliko.ImageLibrary;
    using Kaliko.ImageLibrary.Filters;

    using Main.Core.Commands.File;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Services;
    using Main.Core.View;

    using Ncqrs;
    using Ncqrs.Commanding.ServiceModel;

    
    using Questionnaire.Core.Web.Security;

    using RavenQuestionnaire.Core.Views.Event.File;
    using RavenQuestionnaire.Web.Models;

    
    /// <summary>
    /// The resource controller.
    /// </summary>
    public class ResourceController : Controller
    {
        #region Constants and Fields

        // private ICommandInvoker commandInvoker;

        /// <summary>
        /// The command service.
        /// </summary>
        private readonly ICommandService commandService;

        /// <summary>
        /// The file storage service.
        /// </summary>
        private readonly IFileStorageService fileStorageService;

        /// <summary>
        /// The view repository.
        /// </summary>
        private readonly IViewRepository viewRepository;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceController"/> class.
        /// </summary>
        /// <param name="fileStorageService">
        /// The file storage service.
        /// </param>
        /// <param name="viewRepository">
        /// The view repository.
        /// </param>
        public ResourceController(IFileStorageService fileStorageService, IViewRepository viewRepository)
        {
            // this.commandInvoker = commandInvoker;
            this.viewRepository = viewRepository;
            this.fileStorageService = fileStorageService;
            this.commandService = NcqrsEnvironment.Get<ICommandService>();
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// Delete view
        /// </returns>
        [HttpGet]
        public ActionResult Delete(string id)
        {
            // var filename = id;
            try
            {
                this.commandService.Execute(new DeleteFileCommand(Guid.Parse(id)));
            }
            catch (Exception exception)
            {
                LogManager.GetLogger(this.GetType()).Error("Can't delete image. " + exception.Message);
            }

            return this.RedirectToAction("Index", "Resource", new FileBrowseInputModel());
        }

        /// <summary>
        /// The images.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// Image file
        /// </returns>
        [HttpGet]
        public ActionResult Images(string id)
        {
            Stream fileBytes = this.fileStorageService.RetrieveFile(id).Content;
            return this.File(fileBytes, "image/png");
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// File browse view
        /// </returns>
        public ActionResult Index(FileBrowseInputModel input)
        {
            FileBrowseView model = this.viewRepository.Load<FileBrowseInputModel, FileBrowseView>(input);
            return this.View(model);
        }

        /// <summary>
        /// The thumb.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// Thumbnail image
        /// </returns>
        [HttpGet]
        public ActionResult Thumb(string id)
        {
            Stream fileBytes = this.fileStorageService.RetrieveThumb(id).Content;
            return this.File(fileBytes, "image/png");
        }

        /// <summary>
        /// The update desc.
        /// </summary>
        /// <param name="meta">
        /// The meta.
        /// </param>
        /// <returns>
        /// Update status in JSON format
        /// </returns>
        [HttpPost]
        public ActionResult UpdateDesc(FileBrowseItemClient meta)
        {
            try
            {
                var command = new UpdateFileMetaCommand(Guid.Parse(meta.Id), meta.Title, meta.Description);
                this.commandService.Execute(command);
                return this.Json(new { message = "saved" });
            }
            catch (Exception ex)
            {
                return this.Json(new { message = ex.Message });
            }
        }

        /// <summary>
        /// The upload.
        /// </summary>
        /// <returns>
        /// Upload file view
        /// </returns>
        public ActionResult Upload()
        {
            return this.View();
        }

        // DONT USE THIS IF YOU NEED TO ALLOW LARGE FILES UPLOADS

        /// <summary>
        /// The upload files.
        /// </summary>
        /// <returns>
        /// Upload file status
        /// </returns>
        /// <exception cref="HttpRequestValidationException">
        /// Ecception if file size is too big
        /// </exception>
        [HttpPost]
        public ActionResult UploadFiles()
        {
            var r = new List<ViewDataUploadFilesResult>();

            foreach (string file in this.Request.Files)
            {
                var statuses = new List<ViewDataUploadFilesResult>();
                NameValueCollection headers = this.Request.Headers;

                if (string.IsNullOrEmpty(headers["X-File-Name"]))
                {
                    this.UploadWholeFile(this.Request, statuses);
                }
                else
                {
                    throw new HttpRequestValidationException("Attempt to upload too big file");
                }

                JsonResult result = this.Json(statuses);
                result.ContentType = "text/plain";

                return result;
            }

            return this.Json(r);
        }

        #endregion

        // DONT USE THIS IF YOU NEED TO ALLOW LARGE FILES UPLOADS
        #region Methods

        /// <summary>
        /// The resize image.
        /// </summary>
        /// <param name="image">
        /// The image.
        /// </param>
        /// <param name="width">
        /// The width.
        /// </param>
        /// <param name="height">
        /// The height.
        /// </param>
        /// <param name="newWidth">
        /// The new width.
        /// </param>
        /// <param name="newHeight">
        /// The new height.
        /// </param>
        /// <returns>
        /// Resize and crop image
        /// </returns>
        private MemoryStream ResizeImage(KalikoImage image, int width, int height, out int newWidth, out int newHeight)
        {
            KalikoImage thumb = image.GetThumbnailImage(width, height, ThumbnailMethod.Fit);
            thumb.ApplyFilter(new UnsharpMaskFilter(1.4, 0.32));

            var ms = new MemoryStream();
            thumb.SavePng(ms, 80);
            ms.Position = 0;

            // var thumbData = new byte[ms.Length];
            // ms.Read(thumbData, 0, thumbData.Length);
            newHeight = thumb.Height;
            newWidth = thumb.Width;

            return ms;
        }

        /// <summary>
        /// The upload whole file.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <param name="statuses">
        /// The statuses.
        /// </param>
        private void UploadWholeFile(HttpRequestBase request, List<ViewDataUploadFilesResult> statuses)
        {
            for (int i = 0; i < request.Files.Count; i++)
            {
                string title = request["title"];
                string desc = request["desc"];
                HttpPostedFileBase file = request.Files[i];
                var command = new UploadFileCommand(Guid.NewGuid(), title, desc, file.InputStream);

                this.commandService.Execute(command);

                file.InputStream.Position = 0;
                var image = new KalikoImage(file.InputStream);
                int thumbWidth, thumbHeight;
                MemoryStream thumbData = this.ResizeImage(image, 160, 120, out thumbWidth, out thumbHeight);
                var bytes = new byte[thumbData.Length];
                thumbData.Read(bytes, 0, (int)thumbData.Length);
                statuses.Add(
                    new ViewDataUploadFilesResult
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

        #endregion
    }
}