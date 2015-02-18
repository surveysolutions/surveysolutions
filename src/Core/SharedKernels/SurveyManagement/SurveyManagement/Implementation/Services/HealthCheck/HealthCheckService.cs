using System.Collections.Generic;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.HealthCheck.Checks;
using WB.Core.SharedKernels.SurveyManagement.Services.HealthCheck;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.HealthCheck
{
    class HealthCheckService : IHealthCheckService
    {
        private readonly IAtomicHealthCheck<RavenHealthCheckResult> databaseHealthCheck;
        private readonly IAtomicHealthCheck<EventStoreHealthCheckResult> eventStoreHealthCheck;
        private readonly IAtomicHealthCheck<NumberOfSyncPackagesWithBigSizeCheckResult> numberOfSyncPackagesWithBigSizeChecker;
        private readonly IAtomicHealthCheck<FolderPermissionCheckResult> folderPermissionChecker;
        private readonly IAtomicHealthCheck<NumberOfUnhandledPackagesHealthCheckResult> numberOfUnhandledPackagesChecker;

        public HealthCheckService(
            IAtomicHealthCheck<RavenHealthCheckResult> databaseHealthCheck,
            IAtomicHealthCheck<EventStoreHealthCheckResult> eventStoreHealthCheck,
            IAtomicHealthCheck<NumberOfUnhandledPackagesHealthCheckResult> numberOfUnhandledPackagesChecker,
            IAtomicHealthCheck<NumberOfSyncPackagesWithBigSizeCheckResult> numberOfSyncPackagesWithBigSizeChecker,
            IAtomicHealthCheck<FolderPermissionCheckResult> folderPermissionChecker)
        {
            this.folderPermissionChecker = folderPermissionChecker;
            this.numberOfSyncPackagesWithBigSizeChecker = numberOfSyncPackagesWithBigSizeChecker;
            this.eventStoreHealthCheck = eventStoreHealthCheck;
            this.databaseHealthCheck = databaseHealthCheck;
            this.numberOfUnhandledPackagesChecker = numberOfUnhandledPackagesChecker;
        }

        public HealthCheckResults Check()
        {
            return GetHealthCheckModel();
        }

        private HealthCheckResults GetHealthCheckModel()
        {
            var databaseHealthCheckResult = databaseHealthCheck.Check();
            var eventStoreHealthCheckResult = eventStoreHealthCheck.Check();
            var numberOfUnhandledPackages = numberOfUnhandledPackagesChecker.Check();
            var numberOfSyncPackagesWithBigSize = numberOfSyncPackagesWithBigSizeChecker.Check();
            var folderPermissionCheckResult = folderPermissionChecker.Check();

            HealthCheckStatus status = GetGlobalStatus(databaseHealthCheckResult, eventStoreHealthCheckResult,
                numberOfUnhandledPackages, numberOfSyncPackagesWithBigSize, folderPermissionCheckResult);

            return new HealthCheckResults(status, databaseHealthCheckResult, eventStoreHealthCheckResult,
                numberOfUnhandledPackages, numberOfSyncPackagesWithBigSize, folderPermissionCheckResult);
        }

        private HealthCheckStatus GetGlobalStatus(RavenHealthCheckResult databaseHealthCheckResult, 
            EventStoreHealthCheckResult eventStoreHealthCheckResult, 
            NumberOfUnhandledPackagesHealthCheckResult numberOfUnhandledPackages, 
            NumberOfSyncPackagesWithBigSizeCheckResult numberOfSyncPackagesWithBigSize, 
            FolderPermissionCheckResult folderPermissionCheckResult)
        {
            HashSet<HealthCheckStatus> statuses = new HashSet<HealthCheckStatus>(
            new[] {
                    databaseHealthCheckResult.Status,
                    eventStoreHealthCheckResult.Status,
                    folderPermissionCheckResult.Status,
                    numberOfUnhandledPackages.Status,
                    numberOfSyncPackagesWithBigSize.Status
                });

            if (statuses.Contains(HealthCheckStatus.Down))
                return HealthCheckStatus.Down;
            if (statuses.Contains(HealthCheckStatus.Warning))
                return HealthCheckStatus.Warning;

            return HealthCheckStatus.Happy;
        }
    }
}