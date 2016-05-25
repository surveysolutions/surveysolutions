namespace WB.Core.BoundedContexts.Headquarters.ValueObjects.HealthCheck
{
    public class ReadSideHealthCheckResult
    {
        private ReadSideHealthCheckResult(HealthCheckStatus status, string errorMessage = null)
        {
            this.Status = status;
            this.ErrorMessage = errorMessage;
        }

        public HealthCheckStatus Status { get; private set; }
        public string ErrorMessage { get; private set; }

        public static ReadSideHealthCheckResult Happy()
        {
            return new ReadSideHealthCheckResult(HealthCheckStatus.Happy);
        }

        public static ReadSideHealthCheckResult Down(string errorMessage)
        {
            return new ReadSideHealthCheckResult(HealthCheckStatus.Down, errorMessage);
        }
    }
}