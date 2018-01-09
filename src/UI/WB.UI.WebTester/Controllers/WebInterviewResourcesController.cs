using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Web;
using System.Web.Http;
using WB.Core.Infrastructure.PlainStorage;
using WB.UI.Shared.Web.Modules;
using WB.UI.Shared.Web.Services;

namespace WB.UI.WebTester.Controllers
{
    [RoutePrefix("api")]
    public class WebInterviewResourcesController : ApiController
    {
        private readonly IPlainStorageAccessor<QuestionnaireAttachment> attachmentStorage;
        private readonly IImageProcessingService imageProcessingService;

        public WebInterviewResourcesController(IPlainStorageAccessor<QuestionnaireAttachment> attachmentStorage,
            IImageProcessingService imageProcessingService)
        {
            this.attachmentStorage = attachmentStorage ?? throw new ArgumentNullException(nameof(attachmentStorage));
            this.imageProcessingService = imageProcessingService ?? throw new ArgumentNullException(nameof(imageProcessingService));
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

        private string GetQueryStringValue(string key)
        {
            return (this.Request.GetQueryNameValuePairs().Where(query => query.Key == key).Select(query => query.Value))
                .FirstOrDefault();
        }
    }
}