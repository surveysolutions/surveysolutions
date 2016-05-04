namespace WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck
{
    public class ReadSideHealthCheckResult
    {
        private ReadSideHealthCheckResult(HealthCheckStatus status, string errorMessage = null)
        {
            Status = status;
            ErrorMessage = errorMessage;
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