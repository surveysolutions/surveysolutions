using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WB.Services.Export.Ddi;
using WB.Services.Export.Interview;
using WB.Services.Export.Jobs;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services.Processing;
using WB.Services.Export.Services.Processing.Good;
using WB.Services.Export.Tenant;

namespace WB.Services.Export.Host.Controllers
{
    [ApiController]
    public class JobController : ControllerBase
    {
        private readonly IDataExportProcessesService exportProcessesService;
        private readonly IJobsStatusReporting jobsStatusReporting;
        private readonly IDdiMetadataAccessor ddiDdiMetadataAccessor;
        private readonly ILogger<JobController> logger;
        private readonly IExternalFileStorage externalFileStorage;
        private readonly IDataExportFileAccessor exportFileAccessor;

        public JobController(IDataExportProcessesService exportProcessesService,
        //    IDataExportStatusReader dataExportStatusReader,
            IJobsStatusReporting jobsStatusReporting,
            IDdiMetadataAccessor ddiDdiMetadataAccessor,
            ILogger<JobController> logger,
            IExternalFileStorage externalFileStorage,
            IDataExportFileAccessor exportFileAccessor)
        {
            this.exportProcessesService = exportProcessesService;
            this.jobsStatusReporting = jobsStatusReporting;
            this.ddiDdiMetadataAccessor = ddiDdiMetadataAccessor;
            this.logger = logger;
            this.externalFileStorage = externalFileStorage;
            this.exportFileAccessor = exportFileAccessor;
        }

        [HttpPut]
        [Route("api/v1/job/generate")]
        public ActionResult RequestUpdate(string questionnaireId,
            DataExportFormat format,
            InterviewStatus? status,
            DateTime? from,
            DateTime? to,
            string archiveName,
            string archivePassword,
            string accessToken,
            ExternalStorageType? storageType,
            string apiKey,
            [FromHeader(Name = "Origin")]string tenantBaseUrl)
        {
            if (string.IsNullOrWhiteSpace(archiveName))
            {
                return BadRequest("ArchiveName is required");
            }

            var args = new DataExportProcessDetails(format, new QuestionnaireId(questionnaireId), null)
            {
                Tenant = new TenantInfo(tenantBaseUrl, apiKey),
                InterviewStatus = status,
                FromDate = from,
                ToDate = to,
                ArchivePassword = archivePassword,
                ArchiveName = archiveName,
                AccessToken = accessToken,
                StorageType = storageType
            };

            exportProcessesService.AddDataExport(args);

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
            string apiKey,
            [FromHeader(Name = "Origin")]string tenantBaseUrl)
        {
            var tenant = new TenantInfo(tenantBaseUrl, apiKey);

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
            string apiKey,
            [FromHeader(Name = "Origin")]string tenantBaseUrl)
        {
            var tenant = new TenantInfo(tenantBaseUrl, apiKey);
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
            string apiKey,
            [FromHeader(Name = "Origin")] string baseUrl)
        {
            var tenant = new TenantInfo(baseUrl, apiKey);

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

            ///return this.File(result.Data, "application/octet-stream", result.FileName);
        }
    }
}
