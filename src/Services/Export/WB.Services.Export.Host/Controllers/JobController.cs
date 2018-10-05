using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WB.Services.Export.Ddi;
using WB.Services.Export.Interview;
using WB.Services.Export.Jobs;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services.Processing;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Host.Controllers
{
    [ApiController]
    public class JobController : ControllerBase
    {
        private readonly IDataExportProcessesService exportProcessesService;
        private readonly IJobsStatusReporting jobsStatusReporting;
        private readonly IDdiMetadataAccessor ddiDdiMetadataAccessor;

        public JobController(IDataExportProcessesService exportProcessesService,
            IJobsStatusReporting jobsStatusReporting,
            IDdiMetadataAccessor ddiDdiMetadataAccessor)
        {
            this.exportProcessesService = exportProcessesService;
            this.jobsStatusReporting = jobsStatusReporting;
            this.ddiDdiMetadataAccessor = ddiDdiMetadataAccessor;
        }

        [HttpPut]
        [Route("api/v1/job/generate")]
        public async Task<ActionResult> RequestUpdate(string questionnaireId,
            DataExportFormat format,
            InterviewStatus? status,
            DateTime? from,
            DateTime? to,
            string archiveName,
            string archivePassword,
            string accessToken,
            ExternalStorageType? storageType,
            TenantInfo tenant)
        {
            if (string.IsNullOrWhiteSpace(archiveName))
            {
                return BadRequest("ArchiveName is required");
            }

            var args = new DataExportProcessArgs
            {
                Format = format,
                Questionnaire = new QuestionnaireId(questionnaireId),
                Tenant = tenant,
                InterviewStatus = status,
                FromDate = from,
                ToDate = to,
                ArchivePassword = archivePassword,
                ArchiveName = archiveName,
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
            string archiveName,
            InterviewStatus? status,
            DateTime? fromDate,
            DateTime? toDate,
            TenantInfo tenant)
        {
            var dataExportStatusForQuestionnaire = await this.jobsStatusReporting.GetDataExportStatusForQuestionnaire(tenant,
                new QuestionnaireId(questionnaireId),
                archiveName, status, fromDate, toDate);

            return dataExportStatusForQuestionnaire;
        }

        [HttpGet]
        [ResponseCache(NoStore = true)]
        [Route("api/v1/ddi")]
        public async Task<FileStreamResult> GetDdiFile(
            string questionnaireId,
            string archivePassword,
            TenantInfo tenant)
        {
            var pathToFile = await this.ddiDdiMetadataAccessor.GetFilePathToDDIMetadata(tenant, new QuestionnaireId(questionnaireId),
                archivePassword);
            var responseStream = System.IO.File.OpenRead(pathToFile);
            return File(responseStream, "application/zip");
        }

        [HttpGet]
        [ResponseCache(NoStore = true)]
        [Route("api/v1/job/download")]
        public async Task<ActionResult> DownloadArchive(string questionnaireId,
            string archiveName,
            InterviewStatus? status,
            DataExportFormat format,
            DateTime? fromDate,
            DateTime? toDate,
            TenantInfo tenant)
        {
            var result = await this.jobsStatusReporting.DownloadArchive(tenant, archiveName, format, status, fromDate, toDate);

            if (result == null)
            {
                return NotFound();
            }

            if (result.Redirect != null)
            {
                Response.Headers.Add("NewLocation", result.Redirect.ToString());
                return Ok();
            }

            return new FileStreamResult(result.Data, "application/octet-stream")
            {
                FileDownloadName = result.FileName
            };
        }

        [HttpDelete]
        [Route("api/v1/delete")]
        public ActionResult Delete(TenantInfo tenant)
        {
            return Ok();
        }

        [HttpDelete]
        [Route("api/v1/job")]
        public ActionResult DeleteDataExportProcess(string processId, TenantInfo tenant)
        {
            this.exportProcessesService.DeleteDataExport(tenant, processId);
            return Ok();
        }

        private string apiKey => this.Request.Headers["Authorization"].ToString()?.Replace("Bearer ", "");
    }
}
