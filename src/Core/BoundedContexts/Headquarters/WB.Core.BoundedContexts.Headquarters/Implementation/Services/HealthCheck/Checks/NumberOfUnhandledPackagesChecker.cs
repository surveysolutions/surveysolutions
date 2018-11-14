using System;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.HealthCheck;
using WB.Enumerator.Native.WebInterview;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services.HealthCheck.Checks
{
    class NumberOfUnhandledPackagesChecker : IAtomicHealthCheck<NumberOfUnhandledPackagesHealthCheckResult>
    {
        private readonly IInterviewBrokenPackagesService interviewBrokenPackagesService;

        public NumberOfUnhandledPackagesChecker(IInterviewBrokenPackagesService interviewBrokenPackagesService)
        {
            this.interviewBrokenPackagesService = interviewBrokenPackagesService;
        }

        public NumberOfUnhandledPackagesHealthCheckResult Check()
        {
            try
            {
                int count = this.interviewBrokenPackagesService.InvalidPackagesCount;

                if (count == 0)
                    return NumberOfUnhandledPackagesHealthCheckResult.Happy(count);

                return NumberOfUnhandledPackagesHealthCheckResult.Warning(count,
                    "The error occurred during processing of the interview.<br />Please, contact Survey Solutions Team <a href='mailto:support@mysurvey.solutions'>support@mysurvey.solutions</a> to inform about the issue.");
            }
            catch (Exception e)
            {
                return NumberOfUnhandledPackagesHealthCheckResult.Error("The information about unhandled packages can't be collected. " + e.Message);
            }
        }
    }
}
