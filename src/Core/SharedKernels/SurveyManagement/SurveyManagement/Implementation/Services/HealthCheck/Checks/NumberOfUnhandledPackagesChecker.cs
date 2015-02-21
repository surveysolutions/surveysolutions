using System;
using System.Linq;
using WB.Core.SharedKernels.SurveyManagement.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.HealthCheck.Checks
{
    class NumberOfUnhandledPackagesChecker : IAtomicHealthCheck<NumberOfUnhandledPackagesHealthCheckResult>
    {
        private readonly IBrokenSyncPackagesStorage brokenSyncPackagesStorage;

        public NumberOfUnhandledPackagesChecker(IBrokenSyncPackagesStorage brokenSyncPackagesStorage)
        {
            this.brokenSyncPackagesStorage = brokenSyncPackagesStorage;
        }

        public NumberOfUnhandledPackagesHealthCheckResult Check()
        {
            try
            {
                int count = brokenSyncPackagesStorage.GetListOfUnhandledPackages().Count();

                if (count == 0)
                    return NumberOfUnhandledPackagesHealthCheckResult.Happy(count);

                return NumberOfUnhandledPackagesHealthCheckResult.Warning(count,
                    "The error occurred during interview processing. Please contact Survey Solutions Team <a href='support@mysurvey.solutions'>support@mysurvey.solutions</a> to report the error.");
            }
            catch (Exception e)
            {
                return NumberOfUnhandledPackagesHealthCheckResult.Error("The information about unhandled packages can't be collected. " + e.Message);
            }
        }
    }
}