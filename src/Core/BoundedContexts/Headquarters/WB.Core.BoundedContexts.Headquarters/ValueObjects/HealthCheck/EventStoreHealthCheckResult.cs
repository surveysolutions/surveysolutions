namespace WB.Core.BoundedContexts.Headquarters.ValueObjects.HealthCheck
{
    public class EventStoreHealthCheckResult
    {
        private EventStoreHealthCheckResult(HealthCheckStatus status, string errorMessage = null)
        {
            this.Status = status;
            this.ErrorMessage = errorMessage;
        }

        public HealthCheckStatus Status { get; private set; }
        public string ErrorMessage { get; private set; }

        public static EventStoreHealthCheckResult Happy()
        {
            return new EventStoreHealthCheckResult(HealthCheckStatus.Happy);
        }

        public static EventStoreHealthCheckResult Down(string errorMessage)
        {
            return new EventStoreHealthCheckResult(HealthCheckStatus.Down, errorMessage);
        }
    }
}