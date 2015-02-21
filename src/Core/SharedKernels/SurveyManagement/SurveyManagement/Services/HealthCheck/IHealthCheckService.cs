using WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck;

namespace WB.Core.SharedKernels.SurveyManagement.Services.HealthCheck
{
    public interface IHealthCheckService
    {
        HealthCheckResults Check();
    }
}