using WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck;

namespace WB.Core.SharedKernels.SurveyManagement.Services.HealthCheck.Checks
{
    internal interface IDatabaseHealthCheck 
    {
        ConnectionHealthCheckResult Check();
    }
}