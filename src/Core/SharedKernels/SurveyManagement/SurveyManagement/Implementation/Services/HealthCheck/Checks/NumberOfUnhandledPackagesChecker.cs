using System;
using System.Linq;
using WB.Core.SharedKernels.SurveyManagement.Services.HealthCheck.Checks;
using WB.Core.SharedKernels.SurveyManagement.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.HealthCheck
{
    class NumberOfUnhandledPackagesChecker : INumberOfUnhandledPackagesChecker
    {
        private readonly IBrokenSyncPackagesStorage brokenSyncPackagesStorage;

        public NumberOfUnhandledPackagesChecker(IBrokenSyncPackagesStorage brokenSyncPackagesStorage)
        {
            this.brokenSyncPackagesStorage = brokenSyncPackagesStorage;
        }

        public NumberHealthCheckResult Check()
        {
            try
            {
                int count = brokenSyncPackagesStorage.GetListOfUnhandledPackages().Count();

                if (count == 0)
                    return NumberHealthCheckResult.Happy(count);

                return NumberHealthCheckResult.Warning(count,
                    "The error occurred during interview processing. Please contact Survey Solutions Team <a href='support@mysurvey.solutions'>support@mysurvey.solutions</a> to report the error.");
            }
            catch (Exception e)
            {
                return NumberHealthCheckResult.Error("The information about unhandled packages can't be collected. " + e.Message);
            }
        }
    }
}