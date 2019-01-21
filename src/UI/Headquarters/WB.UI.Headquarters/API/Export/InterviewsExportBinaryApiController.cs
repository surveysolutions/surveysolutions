using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using WB.Core.GenericSubdomains.Portable;
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
        private readonly IAudioAuditFileStorage audioAuditFileStorage;

        public InterviewsExportBinaryApiController(
            IExternalFileStorage fileStorage,
            IImageFileStorage imageFileStorage,
            IAudioFileStorage audioFileStorage,
            IAudioAuditFileStorage audioAuditFileStorage)
        {
            this.fileStorage = fileStorage;
            this.imageFileStorage = imageFileStorage;
            this.audioFileStorage = audioFileStorage;
            this.audioAuditFileStorage = audioAuditFileStorage;
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

        [Route("api/export/v1/interview/{interviewId}/audio/{fileName}")]
        [ServiceApiKeyAuthorization]
        [HttpGet]
        [ApiNoCache]
        public HttpResponseMessage GetInterviewAudio(Guid interviewId, string fileName)
        {
            var descriptors = this.audioFileStorage.GetBinaryFilesForInterview(interviewId);
            var file = descriptors.FirstOrDefault(d => d.FileName == fileName);

            if (file == null) return Request.CreateResponse(HttpStatusCode.NotFound);

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new ByteArrayContent(file.GetData());
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? @"audio/mpeg");
            return response;
        }

        [Route("api/export/v1/interviews/batch/audioAudit")]
        [ServiceApiKeyAuthorization]
        [HttpGet]
        [ApiNoCache]
        public HttpResponseMessage GetAudioAuditDescriptorsForInterviews([FromUri]Guid[] id)
        {
            var audioAuditInfo = id.Select(interviewId =>
            {
                var audioAuditRecords = audioAuditFileStorage.GetBinaryFilesForInterview(interviewId);
                return new
                {
                    InterviewId = interviewId,
                    Files = audioAuditRecords.Select(descriptor =>
                        new
                        {
                            FileName = descriptor.FileName,
                            ContentType = descriptor.ContentType
                        }
                    ).OrderBy(x => x.FileName).ToArray()
                };
            }).OrderBy(x => x.InterviewId).ToArray();

            return Request.CreateResponse(HttpStatusCode.OK, audioAuditInfo);
        }

        [Route("api/export/v1/interview/{interviewId}/audioAudit/{fileName}")]
        [ServiceApiKeyAuthorization]
        [HttpGet]
        [ApiNoCache]
        public HttpResponseMessage GetAudioAudit(Guid interviewId, string fileName)
        {
            var descriptors = this.audioAuditFileStorage.GetBinaryFilesForInterview(interviewId);
            var file = descriptors.FirstOrDefault(d => d.FileName == fileName);

            if (file == null) return Request.CreateResponse(HttpStatusCode.NotFound);

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new ByteArrayContent(file.GetData());
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            return response;
        }
    }
}
