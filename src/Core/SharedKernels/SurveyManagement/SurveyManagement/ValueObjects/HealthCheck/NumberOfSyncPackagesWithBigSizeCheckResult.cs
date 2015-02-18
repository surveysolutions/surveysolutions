namespace WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck
{
    public class NumberOfSyncPackagesWithBigSizeCheckResult
    {
        private NumberOfSyncPackagesWithBigSizeCheckResult(HealthCheckStatus status, int value, string errorMessage = null)
        {
            Status = status;
            ErrorMessage = errorMessage;
            Value = value;
        }

        public HealthCheckStatus Status { get; private set; }
        public int Value { get; private set; }
        public string ErrorMessage { get; private set; }

        public static NumberOfSyncPackagesWithBigSizeCheckResult Happy(int value)
        {
            return new NumberOfSyncPackagesWithBigSizeCheckResult(HealthCheckStatus.Happy, value);
        }

        public static NumberOfSyncPackagesWithBigSizeCheckResult Warning(int value, string message)
        {
            return new NumberOfSyncPackagesWithBigSizeCheckResult(HealthCheckStatus.Warning, value, message);
        }

        public static NumberOfSyncPackagesWithBigSizeCheckResult Error(string errorMessage)
        {
            return new NumberOfSyncPackagesWithBigSizeCheckResult(HealthCheckStatus.Down, -1, errorMessage);
        }
    }
}