using WB.Core.Infrastructure.HealthCheck;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models.Api
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

        public static NumberHealthCheckResult ForNumber(int value)
        {
            var status = value == 0 ? HealthCheckStatus.Happy : HealthCheckStatus.Warning;
            return new NumberHealthCheckResult(status, value);
        }

        public static NumberHealthCheckResult Error(string errorMessage)
        {
            return new NumberHealthCheckResult(HealthCheckStatus.Down, -1, errorMessage);
        }
    }
}