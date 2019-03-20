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
        [Route("api/v1/job/generate")]
        public async Task<ActionResult> RequestUpdate(
            string questionnaireId,
            DataExportFormat format,
            InterviewStatus? status,
            DateTime? from,
            DateTime? to,
            string archivePassword,
            string accessToken,
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
                    Status = status
                },
                ArchivePassword = archivePassword,
                AccessToken = accessToken,
                StorageType = storageType
            };

            await exportProcessesService.AddDataExport(args);

            return Ok();
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
            var dataExportStatusForQuestionnaire = await this.jobsStatusReporting.GetDataExportStatusForQuestionnaireAsync(tenant,
                new QuestionnaireId(questionnaireId), status, fromDate, toDate);

            return dataExportStatusForQuestionnaire;
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
            TenantInfo tenant)
        {
            var exportSettings = new ExportSettings
            {
                QuestionnaireId = new QuestionnaireId(questionnaireId),
                ExportFormat = format,
                Status = status,
                Tenant = tenant,
                FromDate = fromDate,
                ToDate = toDate
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

        [HttpDelete]
        [Route("api/v1/job")]
        public async Task<ActionResult> DeleteDataExportProcess(string processId, TenantInfo tenant)
        {
            var job = await jobService.GetJobAsync(tenant, processId, JobStatus.Running, JobStatus.Created);
            if (job != null)
            {
                this.exportProcessesService.DeleteDataExport(job.Id, "User canceled");
            }

            return Ok();
        }

        [HttpGet]
        [Route("api/v1/job/running")]
        public async Task<List<string>> GetRunningJobsList(TenantInfo tenant)
        {
            var jobs = await this.exportProcessesService.GetAllProcesses(tenant);

            return jobs
                .Where(j => j.Status.IsRunning)
                .Select(j => j.ExportSettings.QuestionnaireId.ToString())
                .ToList();
        }
    }
}
