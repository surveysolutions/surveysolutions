using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Headquarters.API.Filters;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.API.Export
{
    public class InterviewsExportBinaryApiController : ApiController
    {
        private readonly IExternalFileStorage fileStorage;
        private readonly IImageFileStorage imageFileStorage;
        private readonly IAudioFileStorage audioFileStorage;

        public InterviewsExportBinaryApiController(
            IExternalFileStorage fileStorage,
            IImageFileStorage imageFileStorage,
            IAudioFileStorage audioFileStorage)
        {
            this.fileStorage = fileStorage;
            this.imageFileStorage = imageFileStorage;
            this.audioFileStorage = audioFileStorage;
        }


        [Route("api/export/v1/interview/{interviewId}/image/{answer}")]
        [ServiceApiKeyAuthorization]
        [HttpGet]
        [ApiNoCache]
        public HttpResponseMessage GetInterviewImage(Guid interviewId, string answer)
        {
            var descriptors = this.imageFileStorage.GetBinaryFilesForInterview(interviewId);
            var file = descriptors.FirstOrDefault(d => d.FileName == answer);
            if (file == null) return Request.CreateResponse(HttpStatusCode.NotFound);

            if (this.fileStorage.IsEnabled())
            {
                var response = Request.CreateResponse(HttpStatusCode.Moved);
                var filePath = this.imageFileStorage.GetPath(file.InterviewId, file.FileName);
                var link = this.fileStorage.GetDirectLink(filePath, TimeSpan.FromSeconds(10));
                response.Headers.Location = new Uri(link);
                return response;
            }
            else
            {
                var response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new ByteArrayContent(file.GetData());
                response.Content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? @"image/png");
                return response;
            }
        }

        [Route("api/export/v1/interview/{interviewId}/audio/{answer}")]
        [ServiceApiKeyAuthorization]
        [HttpGet]
        [ApiNoCache]
        public HttpResponseMessage GetInterviewAudio(Guid interviewId, string answer)
        {
            var descriptors = this.audioFileStorage.GetBinaryFilesForInterview(interviewId);
            var file = descriptors.FirstOrDefault(d => d.FileName == answer);

            if (file == null) return Request.CreateResponse(HttpStatusCode.NotFound);

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new ByteArrayContent(file.GetData());
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? @"audio/mpeg");
            return response;
        }
    }
}
