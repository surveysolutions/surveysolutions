using System;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using WB.Services.Export.Host.Scheduler;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Tenant;

namespace WB.Services.Export.Host.Controllers
{
    [Route("api/v1/{tenant}")]
    [ApiController]
    public class JobController : ControllerBase
    {
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly ICsvExport exporter;
        private readonly IHostingEnvironment hostingEnvironment;
        private readonly TabularExportJob job;

        public JobController(IBackgroundJobClient backgroundJobClient,
            ICsvExport exporter,
            IHostingEnvironment hostingEnvironment,
            TabularExportJob job)
        {
            this.backgroundJobClient = backgroundJobClient ?? throw new ArgumentNullException(nameof(backgroundJobClient));
            this.exporter = exporter;
            this.hostingEnvironment = hostingEnvironment;
            this.job = job;
        }

        [HttpGet]
        public string Get()
        {
            return "Hello world, " + RouteData.Values["tenant"];
        }

        [HttpPut]
        public ActionResult ExportInterviews(string questionnaireId, 
            InterviewStatus? status, 
            DateTime? from, 
            DateTime? to, 
            [FromHeader(Name = "Origin")]string tenantBaseUrl)
        {
            string tenantId = (string)RouteData.Values["tenant"];
            var jobId = backgroundJobClient.Enqueue(() => job.Execute(tenantBaseUrl,
                new TenantId(tenantId),
                new QuestionnaireId(questionnaireId),
                status,
                from,
                to));

            return Ok();
        }
    }
}
