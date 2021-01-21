using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.InterviewDataStorage;
using WB.Services.Scheduler.Services;

namespace WB.Services.Export.Host.Controllers
{
    [ApiController]
    public class TenantController : ControllerBase
    {
        private readonly IQuestionnaireSchemaGenerator questionnaireSchemaGenerator;
        private readonly IJobsArchiver archiver;
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<TenantController> logger;

        public TenantController(
            IQuestionnaireSchemaGenerator questionnaireSchemaGenerator,
            IJobsArchiver archiver,
            IServiceProvider serviceProvider,
            ILogger<TenantController> logger)
        {
            this.questionnaireSchemaGenerator = questionnaireSchemaGenerator;
            this.archiver = archiver;
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        [HttpDelete]
        [Route("api/v1/deleteTenant")]
        public async Task<ActionResult> StopTenant([FromQuery] string? tenant = null)
        {
            var context = serviceProvider.GetService<ITenantContext>();
            
            if (context != null)
            {
                tenant = context.Tenant.Name;
            }
            
            if (string.IsNullOrWhiteSpace(tenant)) return BadRequest("No tenant specified");
            
            this.logger.LogCritical("Export service tenant {tenant} data deleted due to the request", tenant);
            await this.questionnaireSchemaGenerator.DropTenantSchemaAsync(tenant);
            var jobsCount = await this.archiver.ArchiveJobs(tenant);
            
            return Ok(new
            {
                archivedJobs = jobsCount
            });
        }
    }
}
