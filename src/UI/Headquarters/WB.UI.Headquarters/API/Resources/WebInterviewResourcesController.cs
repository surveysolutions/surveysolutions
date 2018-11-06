using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Shared.Web.Extensions;
using WB.UI.Shared.Web.Modules;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Headquarters.API.Resources
{
    [Localizable(false)]
    [System.Web.Http.AllowAnonymous]
    public class WebInterviewResourcesController : ApiController
    {
        private readonly IAuthorizedUser authorizedUser;
        private readonly IImageFileStorage imageFileStorage;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly IImageProcessingService imageProcessingService;
        private readonly IPlainStorageAccessor<AttachmentContent> attachmentStorage;

        public WebInterviewResourcesController(
            IAuthorizedUser authorizedUser,
            IImageFileStorage imageFileStorage,
            IStatefulInterviewRepository statefulInterviewRepository,
            IImageProcessingService imageProcessingService,
            IPlainStorageAccessor<AttachmentContent> attachmentStorage)
        {
            this.authorizedUser = authorizedUser;
            this.imageFileStorage = imageFileStorage;
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.imageProcessingService = imageProcessingService;
            this.attachmentStorage = attachmentStorage;
        }

        [HttpHead]
        [ActionName("Content")]
        public HttpResponseMessage ContentHead([FromUri] string interviewId, [FromUri] string contentId)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId);

            if (!interview.AcceptsInterviewerAnswers() && !this.authorizedUser.CanConductInterviewReview())
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            var attachment = attachmentStorage.GetById(contentId);
            if (attachment == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.NoContent);
            }

            return new ProgressiveDownload(this.Request).HeaderInfoMessage(attachment.Content.LongLength, attachment.ContentType);
        }

        [HttpGet]
        public HttpResponseMessage Content([FromUri] string interviewId, [FromUri] string contentId)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId);

            if (!interview.AcceptsInterviewerAnswers() && !this.authorizedUser.CanConductInterviewReview())
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            var attachment = attachmentStorage.GetById(contentId);
            if (attachment == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.NoContent);
            }

            if (attachment.IsImage())
            {
                var fullSize = GetQueryStringValue("fullSize") != null;

                var resultFile = fullSize
                    ? attachment.Content
                    : this.imageProcessingService.ResizeImage(attachment.Content, 200, 1920);

                return this.BinaryResponseMessageWithEtag(resultFile);
            }

            return new ProgressiveDownload(Request).ResultMessage(new MemoryStream(attachment.Content), attachment.ContentType);
        }

        [HttpGet]
        public HttpResponseMessage Image([FromUri] string interviewId, [FromUri] string questionId,
            [FromUri] string filename)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId);

            if (!interview.AcceptsInterviewerAnswers() 
                && interview.GetMultimediaQuestion(Identity.Parse(questionId)) != null
                && !this.authorizedUser.CanConductInterviewReview())
            {
                return this.Request.CreateResponse(HttpStatusCode.NoContent);
            }

            var file = this.imageFileStorage.GetInterviewBinaryData(interview.Id, filename);

            if (file == null || file.Length == 0)
                return this.Request.CreateResponse(HttpStatusCode.NoContent);

            var fullSize = GetQueryStringValue("fullSize") != null;
            var resultFile = fullSize
                ? file
                : this.imageProcessingService.ResizeImage(file, 200, 1920);

            return this.BinaryResponseMessageWithEtag(resultFile);
        }

        private string GetQueryStringValue(string key)
        {
            return (this.Request.GetQueryNameValuePairs().Where(query => query.Key == key).Select(query => query.Value))
                .FirstOrDefault();
        }
    }
}
