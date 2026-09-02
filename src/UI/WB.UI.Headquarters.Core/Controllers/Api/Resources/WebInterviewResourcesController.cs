using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Storage;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.UI.Headquarters.Filters;
using WB.UI.Shared.Web.Modules;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Headquarters.Controllers.Api.Resources
{
    [Localizable(false)]
    [Route("api/{controller}/{action}")]
    public class WebInterviewResourcesController : ControllerBase
    {
        private readonly IAuthorizedUser authorizedUser;
        private readonly IImageFileStorage imageFileStorage;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly IImageProcessingService imageProcessingService;
        private readonly IPlainStorageAccessor<AttachmentContent> attachmentStorage;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly ILogger<WebInterviewResourcesController> logger;

        public WebInterviewResourcesController(
            IAuthorizedUser authorizedUser,
            IImageFileStorage imageFileStorage,
            IStatefulInterviewRepository statefulInterviewRepository,
            IImageProcessingService imageProcessingService,
            IPlainStorageAccessor<AttachmentContent> attachmentStorage,
            IQuestionnaireStorage questionnaireStorage,
            ILogger<WebInterviewResourcesController> logger)
        {
            this.authorizedUser = authorizedUser;
            this.imageFileStorage = imageFileStorage;
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.imageProcessingService = imageProcessingService;
            this.attachmentStorage = attachmentStorage;
            this.questionnaireStorage = questionnaireStorage;
            this.logger = logger;
        }

        [HttpHead]
        [ActionName("Content")]
        [WebInterviewResourcesAuthorize(InterviewIdQueryString = "interviewId")]
        public IActionResult ContentHead([FromQuery] string interviewId, [FromQuery] string contentId)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId);

            if (interview == null)
            {
                return NotFound();
            }

            if (!interview.AcceptsInterviewerAnswers() && !this.authorizedUser.CanConductInterviewReview())
            {
                return NotFound();
            }

            var attachment = attachmentStorage.GetById(contentId);
            if (attachment == null)
            {
                return NoContent();
            }
          
            var stream = new MemoryStream(attachment.Content);
            return File(stream, attachment.ContentType, enableRangeProcessing: true);
        }

        [HttpGet]
        [WebInterviewResourcesAuthorize(InterviewIdQueryString = "interviewId")]
        public new IActionResult Content([FromQuery] string interviewId, [FromQuery] string contentId)
        {
            return GetAttachmentContentByContentId(interviewId, contentId, 200);
        }

        private IActionResult GetAttachmentContentByContentId(string interviewId, string contentId, int thumbSize)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId);

            if (interview == null)
            {
                return NotFound();
            }

            if (!interview.AcceptsInterviewerAnswers() && !this.authorizedUser.CanConductInterviewReview())
            {
                return NotFound();
            }

            var attachment = attachmentStorage.GetById(contentId);
            if (attachment == null)
            {
                return NotFound();
            }

            if (attachment.IsImage())
            {
                var fullSize = GetQueryStringValue("fullSize") != null;

                var resultFile = fullSize
                    ? attachment.Content
                    : CreateThumbnailOrNull(attachment.Content, thumbSize);

                if (resultFile != null)
                    return this.BinaryResponseMessageWithEtag(resultFile);
            }

            return File(attachment.Content, attachment.ContentType, enableRangeProcessing: true);
        }

        [HttpGet]
        [WebInterviewResourcesAuthorize(InterviewIdQueryString = "interviewId")]
        public async Task<IActionResult> Image([FromQuery] string interviewId, [FromQuery] string questionId)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId);
           
            if (interview == null)
            {
                return NotFound();
            }

            var multimediaQuestion = interview.GetMultimediaQuestion(Identity.Parse(questionId));

            if (!interview.AcceptsInterviewerAnswers() 
                && multimediaQuestion != null
                && !this.authorizedUser.CanConductInterviewReview())
            {
                return NoContent();
            }

            if (multimediaQuestion == null || !multimediaQuestion.IsAnswered())
                return NoContent();

            var fileName = multimediaQuestion.GetAnswer().FileName;
            var file = await this.imageFileStorage.GetInterviewBinaryDataAsync(interview.Id, fileName);

            if (file == null || file.Length == 0)
                return NoContent();

            var thumbnail = CreateThumbnailOrNull(file, 200);
            if (thumbnail == null)
                return DownloadBinaryFile(file, fileName);

            var fullSize = GetQueryStringValue("fullSize") != null;
            var resultFile = fullSize
                ? file
                : thumbnail;
            var contentType = ContentTypeHelper.GetImageContentType(fileName);

            return this.BinaryResponseMessageWithEtag(resultFile, contentType);
        }
        
        [HttpGet]
        [WebInterviewResourcesAuthorize(InterviewIdQueryString = "interviewId")]
        public IActionResult Attachment([FromQuery] string interviewId, [FromQuery] string attachment)
        {
            if (GetAttachmentById(interviewId, attachment, out var attachmentObj) && attachmentObj != null)
                return GetAttachmentContentByContentId(interviewId, attachmentObj.ContentId, 100);
            return NotFound();
        }

        private bool GetAttachmentById(string interviewId, string attachment, out Attachment attachmentObj)
        {
            attachmentObj = null;
            var interview = this.statefulInterviewRepository.Get(interviewId);

            if (interview == null)
                return false;

            var questionnaire = questionnaireStorage.GetQuestionnaireOrThrow(interview.QuestionnaireIdentity, interview.Language);
            var attachmentId = questionnaire.GetAttachmentIdByName(attachment);
            if (!attachmentId.HasValue)
                return false;

            attachmentObj = questionnaire.GetAttachmentById(attachmentId.Value);
            return true;
        }

        [HttpHead]
        [ActionName("Attachment")]
        [WebInterviewResourcesAuthorize(InterviewIdQueryString = "interviewId")]
        public IActionResult AttachmentHead([FromQuery] string interviewId, [FromQuery] string attachment)
        {
            if (GetAttachmentById(interviewId, attachment, out var attachmentObj) && attachmentObj != null)
                return ContentHead(interviewId, attachmentObj.ContentId);
            return NotFound();
        }

        private byte[] CreateThumbnailOrNull(byte[] content, int thumbSize)
        {
            try
            {
                return this.imageProcessingService.ResizeImage(content, thumbSize, 1920);
            }
            catch (Exception exception) when (exception is ImageFormatException || exception is NotSupportedException)
            {
                this.logger.LogWarning(exception,
                    "Thumbnail cannot be created because image format is not supported. Original file is returned");
                return null;
            }
        }

        private FileContentResult DownloadBinaryFile(byte[] content, string fileName)
        {
            this.Response.Headers["X-Content-Type-Options"] = "nosniff";
            return File(content, "application/octet-stream", fileName);
        }

        private string GetQueryStringValue(string key)
        {
            return this.Request.Query[key];
        }
    }
}
