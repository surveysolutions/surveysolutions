namespace WB.Core.Infrastructure.HealthCheck
{
    public class ConnectionHealthCheckResult
    {
        private ConnectionHealthCheckResult(HealthCheckStatus status, string errorMessage = null)
        {
            Status = status;
            ErrorMessage = errorMessage;
        }

        public HealthCheckStatus Status { get; private set; }
        public string ErrorMessage { get; private set; }

        public static ConnectionHealthCheckResult Happy()
        {
            return new ConnectionHealthCheckResult(HealthCheckStatus.Happy);
        }

        public static ConnectionHealthCheckResult Down(string errorMessage)
        {
            return new ConnectionHealthCheckResult(HealthCheckStatus.Down, errorMessage);
        }
    }
}