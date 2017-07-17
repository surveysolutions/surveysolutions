using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Web;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Headquarters.Services;

namespace WB.UI.Headquarters.API.Resources
{
    [Localizable(false)]
    [System.Web.Http.AllowAnonymous]
    public class WebInterviewResourcesController : ApiController
    {
        private readonly IImageFileStorage imageFileStorage;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly IImageProcessingService imageProcessingService;
        private readonly IPlainStorageAccessor<AttachmentContent> attachmentStorage;

        public WebInterviewResourcesController(
            IImageFileStorage imageFileStorage,
            IStatefulInterviewRepository statefulInterviewRepository,
            IImageProcessingService imageProcessingService,
            IPlainStorageAccessor<AttachmentContent> attachmentStorage)
        {
            this.imageFileStorage = imageFileStorage;
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.imageProcessingService = imageProcessingService;
            this.attachmentStorage = attachmentStorage;
        }

        [HttpGet]
        public HttpResponseMessage Content([FromUri] string interviewId, [FromUri] string contentId)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId);

            if (!interview.AcceptsInterviewerAnswers())
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            var attachment = attachmentStorage.GetById(contentId);
            if (attachment == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.NoContent);
            }

            var fullSize = GetQueryStringValue("fullSize") != null;

            var resultFile = fullSize
                ? attachment.Content
                : this.imageProcessingService.ResizeImage(attachment.Content, 200, 1920);

            return GetBinaryMessageWithEtag(resultFile);
        }

        [HttpGet]
        public HttpResponseMessage Image([FromUri]string interviewId, [FromUri]string questionId, [FromUri]string filename)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId);
            
            if (!interview.AcceptsInterviewerAnswers() && interview.GetMultimediaQuestion(Identity.Parse(questionId)) != null)
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

            return this.GetBinaryMessageWithEtag(resultFile);
        }

        private string GetQueryStringValue(string key)
        {
            return (this.Request.GetQueryNameValuePairs().Where(query => query.Key == key).Select(query => query.Value)).FirstOrDefault();
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