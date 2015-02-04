namespace WB.Core.Infrastructure.HealthCheck
{
    public interface IConnectionHealthCheck
    {
        ConnectionHealthCheckResult Check();
    }
}