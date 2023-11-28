using System;
using System.Collections.Concurrent;
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

        public enum StopTenantStatus
        {
            NotStarted = 1,
            Removing,
            Removed,
            Error
        }

        private static ConcurrentDictionary<string, StopTenantStatus> dropTenantStatuses = new();

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

        private static string lastError = "";

        [HttpDelete]
        [Route("api/v1/deleteTenant")]
        public async Task<ActionResult> StopTenant([FromQuery] string? tenant = null)
        {
            var context = serviceProvider.GetService<ITenantContext>();
            
            if (context != null)
            {
                tenant = context.Tenant.Name;
            }
            
            if (string.IsNullOrWhiteSpace(tenant)) 
                return BadRequest("No tenant specified");
            
            this.logger.LogCritical("Export service tenant {tenant} data deleted due to the request", tenant);
            dropTenantStatuses[tenant] = StopTenantStatus.Removing;
            try
            {
                await this.questionnaireSchemaGenerator.DropTenantSchemaAsync(tenant);
                var jobsCount = await this.archiver.ArchiveJobs(tenant);
                dropTenantStatuses[tenant] = StopTenantStatus.Removed;
                return Ok(new
                {
                    archivedJobs = jobsCount
                });
            }
            catch (Exception e)
            {
                this.logger.LogCritical(e, "Fail to drop tenant {tenant}", tenant);
                dropTenantStatuses[tenant] = StopTenantStatus.Error;
                lastError = e.ToString();
                throw;
            }
        }

        
        [HttpGet]
        [Route("api/v1/statusDeleteTenant")]
        public ActionResult StatusStopTenant([FromQuery] string? tenant = null)
        {
            var context = serviceProvider.GetService<ITenantContext>();
            
            if (context != null)
            {
                tenant = context.Tenant.Name;
            }
            
            if (string.IsNullOrWhiteSpace(tenant)) 
                return BadRequest("No tenant specified");

            var status = dropTenantStatuses.GetOrAdd(tenant, StopTenantStatus.NotStarted);
            this.logger.LogCritical("Export service tenant {tenant} data deleted due to the request", tenant);
            
            return Ok(new
            {
                status = status,
                lastError = lastError
            });
        }
    }
}
