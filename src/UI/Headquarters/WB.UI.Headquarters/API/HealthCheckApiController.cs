using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Services.HealthCheck;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.HealthCheck;


namespace WB.Core.SharedKernels.SurveyManagement.Web.Api
{
    [AllowAnonymous]
    public class HealthCheckApiController : ApiController
    {
        private readonly IHealthCheckService healthCheckService;

        public HealthCheckApiController(IHealthCheckService healthCheckService)
        {
            this.healthCheckService = healthCheckService;
        }

        public HealthCheckStatus GetStatus()
        {
            var healthCheckStatus = healthCheckService.Check();
            return healthCheckStatus.Status;
        }

        public HealthCheckResults GetVerboseStatus()
        {
            return healthCheckService.Check();
        }
    }
}