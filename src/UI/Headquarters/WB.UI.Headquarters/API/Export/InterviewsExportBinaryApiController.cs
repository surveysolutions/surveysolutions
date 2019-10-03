using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
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
        private readonly IAssignmentsService assignmentsService;

        public InterviewsExportBinaryApiController(
            IExternalFileStorage fileStorage,
            IImageFileStorage imageFileStorage,
            IAudioFileStorage audioFileStorage,
            IAudioAuditFileStorage audioAuditFileStorage,
            IAssignmentsService assignmentsService)
        {
            this.fileStorage = fileStorage;
            this.imageFileStorage = imageFileStorage;
            this.audioFileStorage = audioFileStorage;
            this.audioAuditFileStorage = audioAuditFileStorage;
            this.assignmentsService = assignmentsService;
        }


        [Route("api/export/v1/interview/{interviewId}/image/{answer}")]
        [ServiceApiKeyAuthorization]
        [HttpGet]
        [ApiNoCache]
        public async Task<HttpResponseMessage> GetInterviewImage(Guid interviewId, string answer)
        {
            var descriptors = await this.imageFileStorage.GetBinaryFilesForInterview(interviewId);
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
                response.Content = new ByteArrayContent(await file.GetData());
                response.Content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? @"image/png");
                return response;
            }
        }

        [Route("api/export/v1/interview/{interviewId}/audio/{fileName}")]
        [ServiceApiKeyAuthorization]
        [HttpGet]
        [ApiNoCache]
        public async Task<HttpResponseMessage> GetInterviewAudio(Guid interviewId, string fileName)
        {
            var descriptors = await this.audioFileStorage.GetBinaryFilesForInterview(interviewId);
            var file = descriptors.FirstOrDefault(d => d.FileName == fileName);

            if (file == null) return Request.CreateResponse(HttpStatusCode.NotFound);

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new ByteArrayContent(await file.GetData());
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? @"audio/mpeg");
            return response;
        }

        [Route("api/export/v1/interviews/batch/audioAudit")]
        [ServiceApiKeyAuthorization]
        [HttpGet]
        [ApiNoCache]
        public async Task<HttpResponseMessage> GetAudioAuditDescriptorsForInterviews([FromUri]Guid[] id)
        {
            List<(Guid InterviewId, object Files)> response = new List<(Guid interviewId, object Files)>();
            foreach (var interviewId in id)
            {
                List<InterviewBinaryDataDescriptor> audioAuditRecords = await audioAuditFileStorage.GetBinaryFilesForInterview(interviewId);
                var results = audioAuditRecords.Select(descriptor =>
                    new 
                    {
                        descriptor.FileName,
                        descriptor.ContentType
                    }
                ).OrderBy(x => x.FileName).ToArray();

                response.Add((interviewId, results));
            }

            var audioAuditInfo = response.OrderBy(x => x.InterviewId);

            return Request.CreateResponse(HttpStatusCode.OK, audioAuditInfo.Select(x => new
            {
                InterviewId = x.InterviewId,
                Files = x.Files
            }));
        }

        [Route("api/export/v1/interview/{interviewId}/audioAudit/{fileName}")]
        [ServiceApiKeyAuthorization]
        [HttpGet]
        [ApiNoCache]
        public async Task<HttpResponseMessage> GetAudioAudit(Guid interviewId, string fileName)
        {
            var descriptors = await this.audioAuditFileStorage.GetBinaryFilesForInterview(interviewId);
            var file = descriptors.FirstOrDefault(d => d.FileName == fileName);

            if (file == null) return Request.CreateResponse(HttpStatusCode.NotFound);

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new ByteArrayContent(await file.GetData());
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            return response;
        }

        [Route("api/export/v1/questionnaire/{id}/audioAudit")]
        [ServiceApiKeyAuthorization]
        [HttpGet]
        [ApiNoCache]
        public HttpResponseMessage DoesSupportAudioAudit(string id)
        {
            var questionnaireIdentity = QuestionnaireIdentity.Parse(id);
            var hasAssignmentWithAudioRecordingEnabled = assignmentsService.HasAssignmentWithAudioRecordingEnabled(questionnaireIdentity);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                HasAssignmentWithAudioRecordingEnabled = hasAssignmentWithAudioRecordingEnabled
            });
        }
    }
}
