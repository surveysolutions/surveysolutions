namespace WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck
{
    public class NumberOfUnhandledPackagesHealthCheckResult
    {
        private NumberOfUnhandledPackagesHealthCheckResult(HealthCheckStatus status, int value, string errorMessage = null)
        {
            Status = status;
            ErrorMessage = errorMessage;
            Value = value;
        }

        public HealthCheckStatus Status { get; private set; }
        public int Value { get; private set; }
        public string ErrorMessage { get; private set; }

        public static NumberOfUnhandledPackagesHealthCheckResult Happy(int value)
        {
            return new NumberOfUnhandledPackagesHealthCheckResult(HealthCheckStatus.Happy, value);
        }

        public static NumberOfUnhandledPackagesHealthCheckResult Warning(int value, string message)
        {
            return new NumberOfUnhandledPackagesHealthCheckResult(HealthCheckStatus.Warning, value, message);
        }

        public static NumberOfUnhandledPackagesHealthCheckResult Error(string errorMessage)
        {
            return new NumberOfUnhandledPackagesHealthCheckResult(HealthCheckStatus.Down, -1, errorMessage);
        }
    }
}