using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Jobs;
using WB.Services.Export.Models;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services.Processing;
using WB.Services.Scheduler.Model;
using WB.Services.Scheduler.Services;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.Host.Controllers
{
    [ApiController]
    public class JobController : ControllerBase
    {
        private readonly IDataExportProcessesService exportProcessesService;
        private readonly IJobsStatusReporting jobsStatusReporting;
        private readonly IExportArchiveHandleService archiveHandleService;
        private readonly IJobService jobService;
        private readonly ITenantContext tenantContext;

        public JobController(IDataExportProcessesService exportProcessesService,
            IJobsStatusReporting jobsStatusReporting,
            IExportArchiveHandleService archiveHandleService,
            IJobService jobService, ITenantContext tenantContext)
        {
            this.exportProcessesService = exportProcessesService ?? throw new ArgumentNullException(nameof(exportProcessesService));
            this.jobsStatusReporting = jobsStatusReporting ?? throw new ArgumentNullException(nameof(jobsStatusReporting));
            this.archiveHandleService = archiveHandleService ?? throw new ArgumentNullException(nameof(archiveHandleService));
            this.jobService = jobService ?? throw new ArgumentNullException(nameof(jobService));
            this.tenantContext = tenantContext;
        }

        [HttpPut]
        [Route("api/v1/job/regenerate")]
        public async Task<DataExportUpdateRequestResult?> RequestUpdate(
            long processId,
            string? archivePassword,
            string? accessToken,
            string? refreshToken)
        {
            var process = await this.exportProcessesService.GetProcessAsync(processId);

            if (process == null) return null;

            var args = new DataExportProcessArgs(new ExportSettings
                (
                    tenant: tenantContext.Tenant,
                    questionnaireId: process.ExportSettings.QuestionnaireId,
                    exportFormat: process.ExportSettings.ExportFormat,
                    fromDate: process.ExportSettings.FromDate,
                    toDate: process.ExportSettings.ToDate,
                    status: process.ExportSettings.Status,
                    translation: process.ExportSettings.Translation,
                    includeMeta: process.ExportSettings.IncludeMeta
                ))
            {
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
            string? archivePassword,
            string? accessToken,
            string? refreshToken,
            Guid? translationId,
            ExternalStorageType? storageType,
            bool? includeMeta)
        {
            var args = new DataExportProcessArgs(new ExportSettings
            (
                tenant: tenantContext.Tenant,
                questionnaireId: new QuestionnaireId(questionnaireId),
                exportFormat: format,
                fromDate: from,
                toDate: to,
                translation: translationId,
                status: status,
                includeMeta: includeMeta
            ))
            {
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
            DateTime? toDate)
        {
            return await this.jobsStatusReporting.GetDataExportStatusForQuestionnaireAsync(
                new QuestionnaireId(questionnaireId), status, fromDate, toDate);
        }

        [HttpGet]
        [ResponseCache(NoStore = true)]
        [Route("api/v1/job/{id}/download")]
        public async Task<ActionResult> DownloadArchive(long id, string archiveName)
        {
            var job = await this.jobService.GetJobAsync(id);

            var settings = JsonConvert.DeserializeObject<DataExportProcessArgs>(job.Args);

            var exportSettings = settings.ExportSettings;
            exportSettings.JobId = job.Id;
            exportSettings.Tenant = tenantContext.Tenant;

            var result = await this.archiveHandleService.DownloadArchiveAsync(exportSettings, archiveName);

            if (result == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(result.Redirect))
            {
                Response.Headers.Add("NewLocation", result.Redirect);
                return Ok();
            }

            return new FileStreamResult(result.Data, "application/octet-stream")
            {
                FileDownloadName = WebUtility.UrlEncode(result.FileName)
            };
        }

        [HttpGet]
        [ResponseCache(NoStore = true)]
        [Route("api/v1/job/download")]
        [Obsolete("Used by HQ prior to 20.12 release")]
        public async Task<ActionResult> DownloadArchive(
            string questionnaireId,
            string archiveName,
            InterviewStatus? status,
            DataExportFormat format,
            DateTime? fromDate,
            DateTime? toDate,
            Guid? translationId,
            bool? includeMeta)
        {
            var exportSettings = new ExportSettings
            (
                questionnaireId: new QuestionnaireId(questionnaireId),
                exportFormat: format,
                status: status,
                tenant: tenantContext.Tenant,
                fromDate: fromDate,
                toDate: toDate,
                translation: translationId,
                includeMeta: includeMeta
            );

            var result = await this.archiveHandleService.DownloadArchiveAsync(exportSettings, archiveName);

            if (result == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(result.Redirect))
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
        public async Task<ActionResult> Delete()
        {
            await this.archiveHandleService.ClearAllExportArchives(tenantContext.Tenant);
            return Ok();
        }

        [HttpGet]
        [Route("api/v1/job/wasExportRecreated")]
        public async Task<bool> WasExportFileRecreated(long processId)
        {
            return await this.jobService.HasMostRecentFinishedJobIdWithSameTag(processId, tenantContext.Tenant);
        }

        [HttpGet]
        [Route("api/v1/job")]
        public async Task<DataExportProcessView?> GetDataExportStatus(long processId)
        {
            return await this.jobsStatusReporting.GetDataExportStatusAsync(processId);
        }

        [HttpGet]
        [Route("api/v1/jobs")]
        public async Task<List<DataExportProcessView>> GetDataExportStatuses([FromQuery] long[] processIds)
        {
            return await this.jobsStatusReporting.GetDataExportStatusesAsync(processIds);
        }

        [HttpDelete]
        [Route("api/v1/job")]
        public async Task<ActionResult> DeleteDataExportProcess(string processId)
        {
            var job = await jobService.GetJobAsync(tenantContext.Tenant, processId);

            if (job != null && (job.Status == JobStatus.Running || job.Status != JobStatus.Created))
            {
                this.exportProcessesService.DeleteDataExport(job.Id, "User canceled");
            }

            return Ok();
        }

        [HttpDelete]
        [Route("api/v1/job/byId")]
        public async Task<ActionResult> DeleteDataExportProcessById(long jobId)
        {
            var job = await jobService.GetJobAsync(jobId);
            if (job == null) return NotFound();

            if (job.Tenant != tenantContext.Tenant.Id.Id) return NotFound();

            this.exportProcessesService.DeleteDataExport(job.Id, "User canceled");

            return Ok();
        }

        [HttpGet]
        [Route("api/v1/job/running")]
        public async Task<List<long>> GetRunningJobsList(CancellationToken token)
        {
            var jobs = await this.exportProcessesService.GetAllProcessesAsync(cancellationToken: token);

            return jobs
                .Where(j => j.Status.IsRunning)
                .Select(j => j.ProcessId)
                .ToList();
        }

        [HttpGet]
        [Route("api/v1/job/byQuery")]
        public async Task<IEnumerable<DataExportProcessView>> GetJobsByQuery(DataExportFormat? exportType,
            InterviewStatus? interviewStatus, string? questionnaireIdentity, DataExportJobStatus? exportStatus,
            bool? hasFile, int? limit, int? offset)
            => await this.jobsStatusReporting.GetDataExportStatusesAsync(exportType, interviewStatus,
                questionnaireIdentity, exportStatus, hasFile, limit, offset);

        [HttpGet]
        [Route("api/v1/job/all")]
        public async Task<List<long>> GetAllJobsList(CancellationToken token)
        {
            var jobs = await this.exportProcessesService.GetAllProcessesAsync(runningOnly: false, cancellationToken: token);
            return jobs.Select(j => j.ProcessId).ToList();
        }
    }
}
