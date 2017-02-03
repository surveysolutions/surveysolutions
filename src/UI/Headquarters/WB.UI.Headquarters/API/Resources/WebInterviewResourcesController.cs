using System;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Web;
using System.Web.Http;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Headquarters.Services;

namespace WB.UI.Headquarters.API.Resources
{
    [Localizable(false)]
    [System.Web.Http.AllowAnonymous]
    public class WebInterviewResourcesController : ApiController
    {
        private readonly IPlainInterviewFileStorage plainInterviewFileStorage;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly IImageProcessingService imageProcessingService;

        public WebInterviewResourcesController(
            IPlainInterviewFileStorage plainInterviewFileStorage,
            IStatefulInterviewRepository statefulInterviewRepository,
            IImageProcessingService imageProcessingService)
        {
            this.plainInterviewFileStorage = plainInterviewFileStorage;
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.imageProcessingService = imageProcessingService;
        }

        [HttpGet]
        public HttpResponseMessage Image([FromUri]string interviewId, [FromUri]string questionId, [FromUri]string filename)
        {
            var fullSize = this.Request.RequestUri.Query.Contains("fullSize");

            var interview = this.statefulInterviewRepository.Get(interviewId);

            if (interview.Status != InterviewStatus.InterviewerAssigned && interview.GetMultimediaQuestion(Identity.Parse(questionId)) != null)
            {
                return this.Request.CreateResponse(HttpStatusCode.NoContent);
            }

            var file = this.plainInterviewFileStorage.GetInterviewBinaryData(interview.Id, filename);
            if (file == null || file.Length == 0)
                return this.Request.CreateResponse(HttpStatusCode.NoContent);

            var resultFile = fullSize
                ? file
                : this.imageProcessingService.ResizeImage(file, 200, 1920);

            var stringEtag = this.GetEtagValue(resultFile);
            var etag = $"\"{stringEtag}\"";

            var incomingEtag = HttpContext.Current.Request.Headers[@"If-None-Match"];

            if (string.Compare(incomingEtag, etag, StringComparison.InvariantCultureIgnoreCase) == 0)
                return new HttpResponseMessage(HttpStatusCode.NotModified);
            
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