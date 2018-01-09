using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Web;
using System.Web.Http;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.UI.WebTester.Controllers
{
    [RoutePrefix("api")]
    public class WebInterviewResourcesController : ApiController
    {
        private readonly IPlainStorageAccessor<QuestionnaireAttachment> attachmentStorage;

        public WebInterviewResourcesController(IPlainStorageAccessor<QuestionnaireAttachment> attachmentStorage)
        {
            this.attachmentStorage = attachmentStorage;
        }

        [HttpGet]
        public HttpResponseMessage Content([FromUri] string interviewId, [FromUri] string contentId)
        {
            var attachment = attachmentStorage.GetById(contentId);
            if (attachment == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.NoContent);
            }

            var resultFile =  attachment.Content;

            return GetBinaryMessageWithEtag(resultFile.Content);
        }

        private HttpResponseMessage GetBinaryMessageWithEtag(byte[] resultFile)
        {
            var stringEtag = this.GetEtagValue(resultFile);
            var etag = $"\"{stringEtag}\"";

            var incomingEtag = HttpContext.Current.Request.Headers[@"If-None-Match"];

            if (string.Compare(incomingEtag, etag, StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                return new HttpResponseMessage(HttpStatusCode.NotModified);
            }

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(resultFile)
            };

            response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(@"image/png");
            response.Headers.ETag = new EntityTagHeaderValue(etag);
            return response;
        }

        
        private string GetEtagValue(byte[] bytes)
        {
            using (var hasher = SHA1.Create())
            {
                var computeHash = hasher.ComputeHash(bytes);
                string hash = BitConverter.ToString(computeHash).Replace("-", "");
                return hash;
            }
        }
    }
}