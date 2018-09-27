using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WB.Services.Export.Interview;
using WB.Services.Export.Jobs;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services.Processing;
using WB.Services.Export.Services.Processing.Good;
using WB.Services.Export.Tenant;

namespace WB.Services.Export.Host.Controllers
{
    [Route("api/v1/job")]
    [ApiController]
    public class JobController : ControllerBase
    {
        private readonly IDataExportProcessesService exportProcessesService;
        private readonly IJobsStatusReporting jobsStatusReporting;
        private readonly ILogger<JobController> logger;

        public JobController(IDataExportProcessesService exportProcessesService,
        //    IDataExportStatusReader dataExportStatusReader,
            IJobsStatusReporting jobsStatusReporting,
            ILogger<JobController> logger)
        {
            this.exportProcessesService = exportProcessesService;
            this.jobsStatusReporting = jobsStatusReporting;
            this.logger = logger;
        }

        [HttpPut]
        [Route("generate")]
        public ActionResult RequestUpdate(string questionnaireId,
            DataExportFormat format,
            InterviewStatus? status,
            DateTime? from,
            DateTime? to, 
            string archiveName, 
            string archivePassword, 
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
                ArchiveName = archiveName
            };

            exportProcessesService.AddDataExport(args);

            return Ok();
        }

        [HttpGet]
        [ResponseCache(NoStore = true)]
        [Route("status")]
        public DataExportStatusView GetDataExportStatusForQuestionnaire(
            string questionnaireId,
            string archiveName,
            InterviewStatus? status,
            DateTime? fromDate,
            DateTime? toDate,
            string apiKey,
            [FromHeader(Name = "Origin")]string tenantBaseUrl)
        {
            var tenant = new TenantInfo(tenantBaseUrl, apiKey);

            var dataExportStatusForQuestionnaire = this.jobsStatusReporting.GetDataExportStatusForQuestionnaire(tenant,
                new QuestionnaireId(questionnaireId),
                archiveName, status, fromDate, toDate);

            return dataExportStatusForQuestionnaire;
        }
    }
}
