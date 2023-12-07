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

        public enum DropTenantStatus
        {
            Unknown = 0,
            NotStarted = 1,
            Removing,
            Removed,
            Error
        }

        private static ConcurrentDictionary<string, DropTenantStatus> dropTenantStatuses = new();

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
        public async Task<ActionResult> DropTenant([FromQuery] string? tenant = null)
        {
            var context = serviceProvider.GetService<ITenantContext>();
            
            if (context != null)
            {
                tenant = context.Tenant.Name;
            }
            
            if (string.IsNullOrWhiteSpace(tenant)) 
                return BadRequest("No tenant specified");
            
            this.logger.LogCritical("Export service receive request to drop tenant {tenant}", tenant);
            dropTenantStatuses[tenant] = DropTenantStatus.Removing;
            try
            {
                await this.questionnaireSchemaGenerator.DropTenantSchemaAsync(tenant);
                var jobsCount = await this.archiver.ArchiveJobs(tenant);
                dropTenantStatuses[tenant] = DropTenantStatus.Removed;
                return Ok(new
                {
                    archivedJobs = jobsCount
                });
            }
            catch (Exception e)
            {
                this.logger.LogCritical(e, "Fail to drop tenant {tenant}", tenant);
                dropTenantStatuses[tenant] = DropTenantStatus.Error;
                throw;
            }
        }

        
        [HttpGet]
        [Route("api/v1/statusDeleteTenant")]
        public ActionResult DroppingTenantStatus([FromQuery] string? tenant = null)
        {
            var context = serviceProvider.GetService<ITenantContext>();
            
            if (context != null)
            {
                tenant = context.Tenant.Name;
            }
            
            if (string.IsNullOrWhiteSpace(tenant)) 
                return BadRequest("No tenant specified");

            var status = dropTenantStatuses.TryGetValue(tenant, out var dropTenantStatus)
                ? dropTenantStatus
                : DropTenantStatus.NotStarted;
            
            return Ok(new
            {
                status = status
            });
        }
    }
}
