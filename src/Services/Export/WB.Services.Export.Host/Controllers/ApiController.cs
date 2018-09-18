using System;
using System.Threading;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WB.Services.Export.Host.Scheduler;
using WB.Services.Export.Interview;
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
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly IDataExportProcessesService exportProcessesService;
        private readonly ILogger<JobController> logger;
        private readonly IDataExportStatusReader dataExportStatusReader;


        public JobController(IBackgroundJobClient backgroundJobClient,
            IDataExportProcessesService exportProcessesService,
            IDataExportStatusReader dataExportStatusReader,
            ILogger<JobController> logger)
        {
            this.backgroundJobClient =
                backgroundJobClient ?? throw new ArgumentNullException(nameof(backgroundJobClient));
            this.exportProcessesService = exportProcessesService;
            this.logger = logger;
            this.dataExportStatusReader = dataExportStatusReader;
        }

        [HttpPut]
        public ActionResult RequestUpdate(string questionnaireId,
            DataExportFormat format, InterviewStatus? status, DateTime? from, DateTime? to, 
            string apiKey,
            [FromHeader(Name = "Origin")]string tenantBaseUrl)
        {
            var args = new DataExportProcessDetails(format, new QuestionnaireId(questionnaireId), null)
            {
                Tenant = new TenantInfo(tenantBaseUrl, apiKey),
                InterviewStatus = status,
                FromDate = from,
                ToDate = to
            };

            exportProcessesService.AddDataExport(args);

            return Ok();
        }
    }
}
