using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.UI.Shared.Web.Modules;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Headquarters.Controllers.Api.Resources
{
    [Localizable(false)]
    [AllowAnonymous]
    [Route("api/{controller}/{action}")]
    public class WebInterviewResourcesController : ControllerBase
    {
        private readonly IAuthorizedUser authorizedUser;
        private readonly IImageFileStorage imageFileStorage;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly IImageProcessingService imageProcessingService;
        private readonly IPlainStorageAccessor<AttachmentContent> attachmentStorage;
        private readonly IQuestionnaireStorage questionnaireStorage;

        public WebInterviewResourcesController(
            IAuthorizedUser authorizedUser,
            IImageFileStorage imageFileStorage,
            IStatefulInterviewRepository statefulInterviewRepository,
            IImageProcessingService imageProcessingService,
            IPlainStorageAccessor<AttachmentContent> attachmentStorage,
            IQuestionnaireStorage questionnaireStorage)
        {
            this.authorizedUser = authorizedUser;
            this.imageFileStorage = imageFileStorage;
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.imageProcessingService = imageProcessingService;
            this.attachmentStorage = attachmentStorage;
            this.questionnaireStorage = questionnaireStorage;
        }

        [HttpHead]
        [ActionName("Content")]
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
        public new IActionResult Content([FromQuery] string interviewId, [FromQuery] string contentId)
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
                    : this.imageProcessingService.ResizeImage(attachment.Content, 200, 1920);

                return this.BinaryResponseMessageWithEtag(resultFile);
            }

            return File(attachment.Content, attachment.ContentType, enableRangeProcessing: true);
        }

        [HttpGet]
        public async Task<IActionResult> Image([FromQuery] string interviewId, [FromQuery] string questionId,
            [FromQuery] string filename)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId);
           
            if (interview == null)
            {
                return NotFound();
            }

            if (!interview.AcceptsInterviewerAnswers() 
                && interview.GetMultimediaQuestion(Identity.Parse(questionId)) != null
                && !this.authorizedUser.CanConductInterviewReview())
            {
                return NoContent();
            }

            var file = await this.imageFileStorage.GetInterviewBinaryDataAsync(interview.Id, filename);

            if (file == null || file.Length == 0)
                return NoContent();

            var fullSize = GetQueryStringValue("fullSize") != null;
            var resultFile = fullSize
                ? file
                : this.imageProcessingService.ResizeImage(file, 200, 1920);

            return this.BinaryResponseMessageWithEtag(resultFile);
        }
        
        [HttpGet]
        [Route("attachment")]
        public IActionResult Attachment([FromQuery] string interviewId, [FromQuery] string attachment)
        {
            if (GetAttachmentById(interviewId, attachment, out var attachmentObj) && attachmentObj != null)
                return Content(interviewId, attachmentObj.ContentId);
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
        [Route("attachment")]
        public IActionResult AttachmentHead([FromQuery] string interviewId, [FromQuery] string attachment)
        {
            if (GetAttachmentById(interviewId, attachment, out var attachmentObj) && attachmentObj != null)
                return ContentHead(interviewId, attachmentObj.ContentId);
            return NotFound();
        }

        private string GetQueryStringValue(string key)
        {
            return this.Request.Query[key];
        }
    }
}
