using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WB.Services.Export.InterviewDataStorage;
using WB.Services.Scheduler.Services;

namespace WB.Services.Export.Host.Controllers
{
    [ApiController]
    public class TenantController : ControllerBase
    {
        private readonly IQuestionnaireSchemaGenerator questionnaireSchemaGenerator;
        private readonly IJobsArchiver archiver;
        private readonly ILogger<TenantController> logger;

        public TenantController(
            IQuestionnaireSchemaGenerator questionnaireSchemaGenerator,
            IJobsArchiver archiver,
            ILogger<TenantController> logger)
        {
            this.questionnaireSchemaGenerator = questionnaireSchemaGenerator;
            this.archiver = archiver;
            this.logger = logger;
        }

        [HttpDelete]
        [Route("api/v1/deleteTenant")]
        public async Task<ActionResult> StopTenant(string tenant)
        {
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
