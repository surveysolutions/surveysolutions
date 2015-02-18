namespace WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck
{
    public class RavenHealthCheckResult
    {
        private RavenHealthCheckResult(HealthCheckStatus status, string errorMessage = null)
        {
            Status = status;
            ErrorMessage = errorMessage;
        }

        public HealthCheckStatus Status { get; private set; }
        public string ErrorMessage { get; private set; }

        public static RavenHealthCheckResult Happy()
        {
            return new RavenHealthCheckResult(HealthCheckStatus.Happy);
        }

        public static RavenHealthCheckResult Down(string errorMessage)
        {
            return new RavenHealthCheckResult(HealthCheckStatus.Down, errorMessage);
        }
    }
}