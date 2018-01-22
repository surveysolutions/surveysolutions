using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Web;
using System.Web.Http;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Shared.Web.Modules;
using WB.UI.Shared.Web.Services;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester.Controllers
{
    [RoutePrefix("api")]
    public class WebInterviewResourcesController : ApiController
    {
        private readonly IPlainStorageAccessor<QuestionnaireAttachment> attachmentStorage;
        private readonly IImageProcessingService imageProcessingService;
        private readonly ICacheStorage<MultimediaFile, string> mediaStorage;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;

        public WebInterviewResourcesController(IPlainStorageAccessor<QuestionnaireAttachment> attachmentStorage,
            IImageProcessingService imageProcessingService,
            ICacheStorage<MultimediaFile, string> mediaStorage, 
            IStatefulInterviewRepository statefulInterviewRepository)
        {
            this.attachmentStorage = attachmentStorage ?? throw new ArgumentNullException(nameof(attachmentStorage));
            this.imageProcessingService = imageProcessingService ?? throw new ArgumentNullException(nameof(imageProcessingService));
            this.mediaStorage = mediaStorage;
            this.statefulInterviewRepository = statefulInterviewRepository;
        }

        [HttpGet]
        public HttpResponseMessage Content([FromUri] string interviewId, [FromUri] string contentId)
        {
            var attachment = attachmentStorage.GetById(contentId);
            if (attachment == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.NoContent);
            }

            var fullSize = GetQueryStringValue("fullSize") != null;

            var resultFile = fullSize
                ? attachment.Content.Content
                : this.imageProcessingService.ResizeImage(attachment.Content.Content, 200, 1920);

            return this.BinaryResponseMessageWithEtag(resultFile);
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