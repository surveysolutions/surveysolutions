using System.Collections.Generic;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.HealthCheck.Checks;
using WB.Core.SharedKernels.SurveyManagement.Services.HealthCheck;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.HealthCheck
{
    class HealthCheckService : IHealthCheckService
    {
        private readonly IAtomicHealthCheck<EventStoreHealthCheckResult> eventStoreHealthCheck;
        private readonly IAtomicHealthCheck<FolderPermissionCheckResult> folderPermissionChecker;
        private readonly IAtomicHealthCheck<NumberOfUnhandledPackagesHealthCheckResult> numberOfUnhandledPackagesChecker;
        private readonly IAtomicHealthCheck<ReadSideHealthCheckResult> readSideHealthChecker;

        public HealthCheckService(
            IAtomicHealthCheck<EventStoreHealthCheckResult> eventStoreHealthCheck,
            IAtomicHealthCheck<NumberOfUnhandledPackagesHealthCheckResult> numberOfUnhandledPackagesChecker,
            IAtomicHealthCheck<FolderPermissionCheckResult> folderPermissionChecker,
            IAtomicHealthCheck<ReadSideHealthCheckResult> readSideHealthChecker)
        {
            this.folderPermissionChecker = folderPermissionChecker;
            this.eventStoreHealthCheck = eventStoreHealthCheck;
            this.numberOfUnhandledPackagesChecker = numberOfUnhandledPackagesChecker;
            this.readSideHealthChecker = readSideHealthChecker;
        }

        public HealthCheckResults Check()
        {
            return GetHealthCheckModel();
        }

        private HealthCheckResults GetHealthCheckModel()
        {
            var eventStoreHealthCheckResult = eventStoreHealthCheck.Check();
            var numberOfUnhandledPackages = numberOfUnhandledPackagesChecker.Check();
            var folderPermissionCheckResult = folderPermissionChecker.Check();

            var readSideHealthCheckResult = readSideHealthChecker.Check();

            HealthCheckStatus status = GetGlobalStatus(eventStoreHealthCheckResult,
                numberOfUnhandledPackages, folderPermissionCheckResult, 
                readSideHealthCheckResult);

            return new HealthCheckResults(status, eventStoreHealthCheckResult,
                numberOfUnhandledPackages, folderPermissionCheckResult, 
                readSideHealthCheckResult);
        }

        private HealthCheckStatus GetGlobalStatus(EventStoreHealthCheckResult eventStoreHealthCheckResult, 
            NumberOfUnhandledPackagesHealthCheckResult numberOfUnhandledPackages, 
            FolderPermissionCheckResult folderPermissionCheckResult,
            ReadSideHealthCheckResult readSideHealthCheckResult)
        {
            HashSet<HealthCheckStatus> statuses = new HashSet<HealthCheckStatus>(
            new[] {
                    eventStoreHealthCheckResult.Status,
                    folderPermissionCheckResult.Status,
                    numberOfUnhandledPackages.Status,
                    readSideHealthCheckResult.Status
                });

            if (statuses.Contains(HealthCheckStatus.Down))
                return HealthCheckStatus.Down;
            if (statuses.Contains(HealthCheckStatus.Warning))
                return HealthCheckStatus.Warning;

            return HealthCheckStatus.Happy;
        }
    }
}