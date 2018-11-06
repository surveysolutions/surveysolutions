using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Shared.Web.Extensions;
using WB.UI.Shared.Web.Modules;
using WB.UI.Shared.Web.Services;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester.Controllers
{
    [RoutePrefix("api")]
    public class WebInterviewResourcesController : ApiController
    {
        private readonly ICacheStorage<QuestionnaireAttachment, string> attachmentStorage;
        private readonly IImageProcessingService imageProcessingService;
        private readonly ICacheStorage<MultimediaFile, string> mediaStorage;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;

        public WebInterviewResourcesController(
            ICacheStorage<QuestionnaireAttachment, string> attachmentStorage,
            IImageProcessingService imageProcessingService,
            ICacheStorage<MultimediaFile, string> mediaStorage, 
            IStatefulInterviewRepository statefulInterviewRepository)
        {
            this.attachmentStorage = attachmentStorage ?? throw new ArgumentNullException(nameof(attachmentStorage));
            this.imageProcessingService = imageProcessingService ?? throw new ArgumentNullException(nameof(imageProcessingService));
            this.mediaStorage = mediaStorage ?? throw new ArgumentNullException(nameof(mediaStorage));
            this.statefulInterviewRepository = statefulInterviewRepository ?? throw new ArgumentNullException(nameof(statefulInterviewRepository));
        }

        [HttpHead]
        [ActionName("Content")]
        public HttpResponseMessage ContentHead([FromUri] string interviewId, [FromUri] string contentId)
        {
            var attachment = attachmentStorage.Get(contentId, Guid.Parse(interviewId));
            if (attachment == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.NoContent);
            }

            return new ProgressiveDownload(this.Request).HeaderInfoMessage(attachment.Content.Content.LongLength, attachment.Content.ContentType);
        }

        [HttpGet]
        public HttpResponseMessage Content([FromUri] string interviewId, [FromUri] string contentId)
        {
            var attachment = attachmentStorage.Get(contentId, Guid.Parse(interviewId));
            if (attachment == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.NoContent);
            }

            if (attachment.Content.IsImage())
            {
                var fullSize = GetQueryStringValue("fullSize") != null;

                var resultFile = fullSize
                    ? attachment.Content.Content
                    : this.imageProcessingService.ResizeImage(attachment.Content.Content, 200, 1920);

                return this.BinaryResponseMessageWithEtag(resultFile);
            }

            return new ProgressiveDownload(Request).ResultMessage(new MemoryStream(attachment.Content.Content), attachment.Content.ContentType);
        }

        [HttpGet]
        public HttpResponseMessage Image([FromUri] string interviewId, [FromUri] string questionId,
            [FromUri] string filename)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId);
            
            var file = this.mediaStorage.Get(filename, interview.Id);

            if ((file?.Data?.Length ?? 0) == 0)
                return this.Request.CreateResponse(HttpStatusCode.NoContent);

            var fullSize = GetQueryStringValue("fullSize") != null;
            var resultFile = fullSize
                ? file.Data
                : this.imageProcessingService.ResizeImage(file.Data, 200, 1920);

            return this.BinaryResponseMessageWithEtag(resultFile);
        }


        private string GetQueryStringValue(string key)
        {
            return (this.Request.GetQueryNameValuePairs().Where(query => query.Key == key).Select(query => query.Value))
                .FirstOrDefault();
        }
    }
}
