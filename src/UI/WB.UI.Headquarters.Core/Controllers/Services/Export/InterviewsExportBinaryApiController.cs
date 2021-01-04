using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.UI.Headquarters.Code.Authentication;

namespace WB.UI.Headquarters.Controllers.Services.Export
{
    [Authorize(AuthenticationSchemes = AuthType.TenantToken)]
    public class InterviewsExportBinaryApiController : Controller
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
        [HttpGet]
        [ApiNoCache]
        public async Task<ActionResult> GetInterviewImage(Guid interviewId, string answer)
        {
            var descriptors = await this.imageFileStorage.GetBinaryFilesForInterview(interviewId);
            var file = descriptors.FirstOrDefault(d => d.FileName == answer);
            if (file == null) return NotFound();

            if (this.fileStorage.IsEnabled())
            {
                
                var filePath = this.imageFileStorage.GetPath(file.InterviewId, file.FileName);
                var link = this.fileStorage.GetDirectLink(filePath, TimeSpan.FromMinutes(10));
                return this.Redirect(link);
            }
            else
            {
                return File(await file.GetData(), file.ContentType ?? "image/png");
            }
        }

        [Route("api/export/v1/interview/{interviewId}/audio/{fileName}")]
        [HttpGet]
        [ApiNoCache]
        public async Task<ActionResult> GetInterviewAudio(Guid interviewId, string fileName)
        {
            var descriptors = await this.audioFileStorage.GetBinaryFilesForInterview(interviewId);
            var file = descriptors.FirstOrDefault(d => d.FileName == fileName);

            if (file == null) return NotFound();

            return File(await file.GetData(), file.ContentType ?? @"audio/mpeg");
        }

        [Route("api/export/v1/interviews/batch/audioAudit")]
        [HttpGet]
        [ApiNoCache]
        public async Task<ActionResult> GetAudioAuditDescriptorsForInterviews([FromQuery]Guid[] id)
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

            return Ok(audioAuditInfo.Select(x => new
            {
                InterviewId = x.InterviewId,
                Files = x.Files
            }));
        }

        [Route("api/export/v1/interview/{interviewId}/audioAudit/{fileName}")]
        [HttpGet]
        [ApiNoCache]
        public async Task<ActionResult> GetAudioAudit(Guid interviewId, string fileName)
        {
            var descriptors = await this.audioAuditFileStorage.GetBinaryFilesForInterview(interviewId);
            var file = descriptors.FirstOrDefault(d => d.FileName == fileName);

            if (file == null) return NotFound();

            return File(await file.GetData(), file.ContentType);
        }

        [Route("api/export/v1/questionnaire/{id}/audioAudit")]
        [HttpGet]
        [ApiNoCache]
        public ActionResult DoesSupportAudioAudit(string id)
        {
            var questionnaireIdentity = QuestionnaireIdentity.Parse(id);
            var hasAssignmentWithAudioRecordingEnabled = assignmentsService.HasAssignmentWithAudioRecordingEnabled(questionnaireIdentity);

            return Ok(new
            {
                HasAssignmentWithAudioRecordingEnabled = hasAssignmentWithAudioRecordingEnabled
            });
        }
    }
}
