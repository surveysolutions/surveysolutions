namespace WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck
{
    public class NumberHealthCheckResult
    {
        private NumberHealthCheckResult(HealthCheckStatus status, int value, string errorMessage = null)
        {
            Status = status;
            ErrorMessage = errorMessage;
            Value = value;
        }

        public HealthCheckStatus Status { get; private set; }
        public int Value { get; private set; }
        public string ErrorMessage { get; private set; }

        public static NumberHealthCheckResult Happy(int value)
        {
            return new NumberHealthCheckResult(HealthCheckStatus.Happy, value);
        }

        public static NumberHealthCheckResult Warning(int value, string message)
        {
            return new NumberHealthCheckResult(HealthCheckStatus.Warning, value, message);
        }

        public static NumberHealthCheckResult Error(string errorMessage)
        {
            return new NumberHealthCheckResult(HealthCheckStatus.Down, -1, errorMessage);
        }
    }
}