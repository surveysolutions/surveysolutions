using System;
using Hangfire;
using Hangfire.Client;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.States;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WB.Services.Export.Host.Scheduler;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Tenant;

namespace WB.Services.Export.Host.Controllers
{
    [Route("api/v1/job")]
    [ApiController]
    public class JobController : ControllerBase
    {
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly ILogger<JobController> logger;

        public JobController(IBackgroundJobClient backgroundJobClient, ILogger<JobController> logger)
        {
            this.backgroundJobClient = backgroundJobClient ?? throw new ArgumentNullException(nameof(backgroundJobClient));
            this.logger = logger;
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
            string apiKey,
            [FromHeader(Name = "Origin")]string tenantBaseUrl)
        {
            var jobId = backgroundJobClient.Enqueue<TabularExportJob>(job => job.Execute(
                new TenantInfo(tenantBaseUrl, apiKey),
                new QuestionnaireId(questionnaireId),
                status,
                from,
                to));

            logger.LogInformation("Enequed job " + jobId);

            return Ok();
        }
    }
}
