using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WB.Services.Export.Interview;
using WB.Services.Export.Jobs;
using WB.Services.Export.Models;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services.Processing;
using WB.Services.Infrastructure.Tenant;
using WB.Services.Scheduler.Model;
using WB.Services.Scheduler.Services;

namespace WB.Services.Export.Host.Controllers
{
    [ApiController]
    public class JobController : ControllerBase
    {
        private readonly IDataExportProcessesService exportProcessesService;
        private readonly IJobsStatusReporting jobsStatusReporting;
        private readonly IExportArchiveHandleService archiveHandleService;
        private readonly IJobService jobService;
        
        public JobController(IDataExportProcessesService exportProcessesService,
            IJobsStatusReporting jobsStatusReporting,
            IExportArchiveHandleService archiveHandleService,
            IJobService jobService)
        {
            this.exportProcessesService = exportProcessesService ?? throw new ArgumentNullException(nameof(exportProcessesService));
            this.jobsStatusReporting = jobsStatusReporting ?? throw new ArgumentNullException(nameof(jobsStatusReporting));
            this.archiveHandleService = archiveHandleService ?? throw new ArgumentNullException(nameof(archiveHandleService));
            this.jobService = jobService ?? throw new ArgumentNullException(nameof(jobService));
        }

        [HttpPut]
        [Route("api/v1/job/regenerate")]
        public async Task<DataExportUpdateRequestResult> RequestUpdate(
            long processId,
            string archivePassword,
            string accessToken,
            string refreshToken,
            TenantInfo tenant)
        {
            var process = await this.exportProcessesService.GetProcessAsync(processId);
            var args = new DataExportProcessArgs
            {
                ExportSettings = new ExportSettings
                {
                    Tenant = tenant,
                    QuestionnaireId = process.ExportSettings.QuestionnaireId,
                    ExportFormat = process.ExportSettings.ExportFormat,
                    FromDate = process.ExportSettings.FromDate,
                    ToDate = process.ExportSettings.ToDate,
                    Status = process.ExportSettings.Status,
                    Translation = process.ExportSettings.Translation
                },
                ArchivePassword = archivePassword,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                StorageType = process.StorageType
            };

            var jobId = await exportProcessesService.AddDataExport(args);

            return new DataExportUpdateRequestResult
            {
                JobId = jobId
            };
        }

        [HttpPut]
        [Route("api/v1/job/generate")]
        public async Task<DataExportUpdateRequestResult> RequestUpdate(
            string questionnaireId,
            DataExportFormat format,
            InterviewStatus? status,
            DateTime? from,
            DateTime? to,
            string archivePassword,
            string accessToken,
            string refreshToken,
            Guid? translationId,
            ExternalStorageType? storageType,
            TenantInfo tenant)
        {
            var args = new DataExportProcessArgs
            {
                ExportSettings = new ExportSettings
                {
                    Tenant = tenant,
                    QuestionnaireId = new QuestionnaireId(questionnaireId),
                    ExportFormat = format,
                    FromDate = from,
                    ToDate = to,
                    Translation = translationId,
                    Status = status
                },
                ArchivePassword = archivePassword,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                StorageType = storageType
            };

            var jobId = await exportProcessesService.AddDataExport(args);

            return new DataExportUpdateRequestResult
            {
                JobId = jobId
            };
        }

        [HttpGet]
        [ResponseCache(NoStore = true)]
        [Route("api/v1/job/status")]
        public async Task<DataExportStatusView> GetDataExportStatusForQuestionnaire(
            string questionnaireId,
            InterviewStatus? status,
            DateTime? fromDate,
            DateTime? toDate,
            TenantInfo tenant)
        {
            return await this.jobsStatusReporting.GetDataExportStatusForQuestionnaireAsync(tenant,
                new QuestionnaireId(questionnaireId), status, fromDate, toDate);
        }


        [HttpGet]
        [ResponseCache(NoStore = true)]
        [Route("api/v1/job/download")]
        public async Task<ActionResult> DownloadArchive(
            string questionnaireId,
            string archiveName,
            InterviewStatus? status,
            DataExportFormat format,
            DateTime? fromDate,
            DateTime? toDate,
            Guid? translationId,
            TenantInfo tenant)
        {
            var exportSettings = new ExportSettings
            {
                QuestionnaireId = new QuestionnaireId(questionnaireId),
                ExportFormat = format,
                Status = status,
                Tenant = tenant,
                FromDate = fromDate,
                ToDate = toDate,
                Translation = translationId
            };

            var result = await this.archiveHandleService.DownloadArchiveAsync(exportSettings, archiveName);

            if (result == null)
            {
                return NotFound();
            }

            if (result.Redirect != null)
            {
                Response.Headers.Add("NewLocation", result.Redirect);
                return Ok();
            }

            return new FileStreamResult(result.Data, "application/octet-stream")
            {
                FileDownloadName = WebUtility.UrlEncode(result.FileName)
            };
        }

        [HttpDelete]
        [Route("api/v1/deleteArchives")]
        public async Task<ActionResult> Delete(TenantInfo tenant)
        {
            await this.archiveHandleService.ClearAllExportArchives(tenant);
            return Ok();
        }

        [HttpGet]
        [Route("api/v1/job/wasExportRecreated")]
        public async Task<bool> WasExportFileRecreated(long processId, TenantInfo tenant)
        {
            return await this.jobService.HasMostRecentFinishedJobIdWithSameTag(processId, tenant);
        }

        [HttpGet]
        [Route("api/v1/job")]
        public async Task<DataExportProcessView> GetDataExportStatus(long processId, TenantInfo tenant)
        {
            return await this.jobsStatusReporting.GetDataExportStatusAsync(processId, tenant);
        }

        [HttpGet]
        [Route("api/v1/jobs")]
        public async Task<List<DataExportProcessView>> GetDataExportStatuses([FromQuery] long[] processIds, TenantInfo tenant)
        {
            return await this.jobsStatusReporting.GetDataExportStatusesAsync(processIds, tenant);
        }

        [HttpDelete]
        [Route("api/v1/job")]
        public async Task<ActionResult> DeleteDataExportProcess(string processId, TenantInfo tenant)
        {
            var job = await jobService.GetJobAsync(tenant, processId);
            
            if (job != null && (job.Status == JobStatus.Running || job.Status != JobStatus.Created))
            {
                this.exportProcessesService.DeleteDataExport(job.Id, "User canceled");
            }

            return Ok();
        }

        [HttpDelete]
        [Route("api/v1/job/byId")]
        public async Task<ActionResult> DeleteDataExportProcessById(long jobId, TenantInfo tenant)
        {
            var job = await jobService.GetJobAsync(jobId);
            if (job == null) return NotFound();

            if (job.Tenant != tenant.Id.Id) return NotFound();

            this.exportProcessesService.DeleteDataExport(job.Id, "User canceled");

            return Ok();
        }

        [HttpGet]
        [Route("api/v1/job/running")]
        public async Task<List<long>> GetRunningJobsList(TenantInfo tenant)
        {
            var jobs = await this.exportProcessesService.GetAllProcesses();

            return jobs
                .Where(j => j.Status.IsRunning)
                .Select(j => j.ProcessId)
                .ToList();
        }

        [HttpGet]
        [Route("api/v1/job/byQuery")]
        public async Task<IEnumerable<DataExportProcessView>> GetJobsByQuery(DataExportFormat? exportType,
            InterviewStatus? interviewStatus, string questionnaireIdentity, DataExportJobStatus? exportStatus, bool? hasFile, int? limit, int? offset, TenantInfo tenant)
            => await this.jobsStatusReporting.GetDataExportStatusesAsync(exportType, interviewStatus,
                questionnaireIdentity, exportStatus, hasFile, limit, offset, tenant);

        [HttpGet]
        [Route("api/v1/job/all")]
        public async Task<List<long>> GetAllJobsList()
        {
            var jobs = await this.exportProcessesService.GetAllProcesses(runningOnly: false);
            return jobs.Select(j => j.ProcessId).ToList();
        }
    }
}
