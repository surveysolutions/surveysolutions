using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WB.Services.Export.InterviewDataStorage;

namespace WB.Services.Export.Host.Controllers
{
    [ApiController]
    public class TenantController : ControllerBase
    {
        private readonly IQuestionnaireSchemaGenerator questionnaireSchemaGenerator;
        private readonly ILogger<TenantController> logger;

        public TenantController(IQuestionnaireSchemaGenerator questionnaireSchemaGenerator,
            ILogger<TenantController> logger)
        {
            this.questionnaireSchemaGenerator = questionnaireSchemaGenerator;
            this.logger = logger;
        }

        [HttpDelete]
        [Route("api/v1/deleteTenant")]
        public async Task<ActionResult> StopTenant(string tenant)
        {
            if (string.IsNullOrWhiteSpace(tenant)) return BadRequest("No tenant specified");
            
            this.logger.LogCritical("Export service tenant {tenant} data deleted due to the request", tenant);
            await this.questionnaireSchemaGenerator.DropTenantSchemaAsync(tenant);
            return Ok();
        }

    }
}
