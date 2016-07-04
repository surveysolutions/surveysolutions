using WB.Core.BoundedContexts.Headquarters.ValueObjects.HealthCheck;

namespace WB.Core.BoundedContexts.Headquarters.Services.HealthCheck
{
    public interface IHealthCheckService
    {
        HealthCheckResults Check();
    }
}