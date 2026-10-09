using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.UI.Shared.Web.Modules;
using WB.UI.Shared.Web.Services;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester.Controllers
{
    [Route("api/{controller}/{action}")]
    public class WebInterviewResourcesController : Controller
    {
        private readonly ICacheStorage<QuestionnaireAttachment, string> attachmentStorage;
        private readonly IImageProcessingService imageProcessingService;
        private readonly ICacheStorage<MultimediaFile, string> mediaStorage;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly IQuestionnaireStorage questionnaireStorage;

        public WebInterviewResourcesController(
            ICacheStorage<QuestionnaireAttachment, string> attachmentStorage,
            IImageProcessingService imageProcessingService,
            ICacheStorage<MultimediaFile, string> mediaStorage,
            IStatefulInterviewRepository statefulInterviewRepository,
            IQuestionnaireStorage questionnaireStorage)
        {
            this.attachmentStorage = attachmentStorage ?? throw new ArgumentNullException(nameof(attachmentStorage));
            this.imageProcessingService = imageProcessingService ?? throw new ArgumentNullException(nameof(imageProcessingService));
            this.mediaStorage = mediaStorage ?? throw new ArgumentNullException(nameof(mediaStorage));
            this.statefulInterviewRepository = statefulInterviewRepository ?? throw new ArgumentNullException(nameof(statefulInterviewRepository));
            this.questionnaireStorage = questionnaireStorage ?? throw new ArgumentNullException(nameof(questionnaireStorage));
        }

        [HttpHead]
        [ActionName("content")]
        public IActionResult ContentHead([FromQuery] string interviewId, [FromQuery] string contentId)
        {
            var attachment = attachmentStorage.Get(contentId, Guid.Parse(interviewId));
            if (attachment?.Content?.Content == null)
            {
                return NoContent();
            }

            var stream = new MemoryStream(attachment.Content.Content);
            return File(stream, attachment.Content.ContentType, enableRangeProcessing: true);
        }

        [HttpGet]
        [ActionName("content")]
        public IActionResult GetContent([FromQuery] string interviewId, [FromQuery] string contentId)
        {
            return GetAttachmentByContentId(interviewId, contentId, 200);
        }

        private IActionResult GetAttachmentByContentId(string interviewId, string contentId, int thumbSize)
        {
            var attachment = attachmentStorage.Get(contentId, Guid.Parse(interviewId));
            if (attachment?.Content?.Content == null)
            {
                return NotFound();
            }

            if (attachment.Content.IsImage())
            {
                var fullSize = GetQueryStringValue("fullSize") != null;
                var content = attachment.Content.Content;

                if (fullSize)
                {
                    var contentType = GetSupportedImageContentType(content);
                    return contentType == null
                        ? DownloadBinaryFile(content, contentId)
                        : this.BinaryResponseMessageWithEtag(content, contentType);
                }

                var thumbnail = TryResizeImage(content, thumbSize);
                return thumbnail == null
                    ? DownloadBinaryFile(content, contentId)
                    : this.BinaryResponseMessageWithEtag(thumbnail, "image/png");
            }

            MemoryStream stream = new MemoryStream(attachment.Content.Content);

            return File(stream, attachment.Content.ContentType, enableRangeProcessing: true);
        }

        [HttpGet]
        [ActionName("image")]
        public IActionResult Image([FromQuery] string interviewId, [FromQuery] string questionId,
            [FromQuery] string filename)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId);

            if (interview == null)
            {
                return NotFound();
            }

            MultimediaFile? file = this.mediaStorage.Get(filename, interview.Id);

            if (file == null || (file?.Data?.Length ?? 0) == 0)
                return NoContent();

            var fullSize = GetQueryStringValue("fullSize") != null;
            if (fullSize)
            {
                var contentType = GetSupportedImageContentType(file!.Data);
                return contentType == null
                    ? DownloadBinaryFile(file.Data, file.Filename)
                    : this.BinaryResponseMessageWithEtag(file.Data, contentType);
            }

            var thumbnail = TryResizeImage(file!.Data, 200);
            return thumbnail == null
                ? DownloadBinaryFile(file.Data, file.Filename)
                : this.BinaryResponseMessageWithEtag(thumbnail, "image/png");
        }

        [HttpGet]
        [ActionName("attachment")]
        public IActionResult GetAttachment([FromQuery] string interviewId, [FromQuery] string attachment)
        {
            if (GetAttachmentById(interviewId, attachment, out var attachmentObj) && attachmentObj != null)
                return GetAttachmentByContentId(interviewId, attachmentObj.ContentId, 100);
            return NotFound();
        }

        private bool GetAttachmentById(string interviewId, string attachment, out Attachment? attachmentObj)
        {
            attachmentObj = null;
            var interview = this.statefulInterviewRepository.Get(interviewId);

            if (interview == null)
                return false;

            var questionnaire =
                questionnaireStorage.GetQuestionnaireOrThrow(interview.QuestionnaireIdentity, interview.Language);
            var attachmentId = questionnaire.GetAttachmentIdByName(attachment);
            if (!attachmentId.HasValue)
                return false;

            attachmentObj = questionnaire.GetAttachmentById(attachmentId.Value);
            return true;
        }

        [HttpHead]
        [ActionName("attachment")]
        public IActionResult AttachmentHead([FromQuery] string interviewId, [FromQuery] string attachment)
        {
            if (GetAttachmentById(interviewId, attachment, out var attachmentObj) && attachmentObj != null)
                return ContentHead(interviewId, attachmentObj.ContentId);
            return NotFound();
        }


        private string? GetQueryStringValue(string key)
        {
            return (this.Request.Query.Where(query => query.Key == key).Select(query => query.Value))
                .FirstOrDefault();
        }

        private byte[]? TryResizeImage(byte[] content, int height)
        {
            try
            {
                return this.imageProcessingService.ResizeImage(content, height, 1920);
            }
            catch (Exception exception) when (exception is ImageFormatException || exception is NotSupportedException)
            {
                return null;
            }
        }

        private string? GetSupportedImageContentType(byte[] content)
        {
            try
            {
                this.imageProcessingService.Validate(content);
                return SixLabors.ImageSharp.Image.DetectFormat(content)?.DefaultMimeType;
            }
            catch (Exception exception) when (exception is ImageFormatException || exception is NotSupportedException)
            {
                return null;
            }
        }

        private FileContentResult DownloadBinaryFile(byte[] content, string fileName)
        {
            this.Response.Headers["X-Content-Type-Options"] = "nosniff";
            return File(content, "application/octet-stream", fileName);
        }
    }
}
