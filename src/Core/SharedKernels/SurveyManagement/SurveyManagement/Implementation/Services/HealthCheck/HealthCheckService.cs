using System.Collections.Generic;
using WB.Core.SharedKernels.SurveyManagement.Services.HealthCheck;
using WB.Core.SharedKernels.SurveyManagement.Services.HealthCheck.Checks;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.HealthCheck
{
    class HealthCheckService : IHealthCheckService
    {
        private readonly INumberOfUnhandledPackagesChecker numberOfUnhandledPackagesChecker;
        private readonly IDatabaseHealthCheck databaseHealthCheck;
        private readonly IEventStoreHealthCheck eventStoreHealthCheck;
        private readonly INumberOfSyncPackagesWithBigSizeChecker numberOfSyncPackagesWithBigSizeChecker;
        private readonly IFolderPermissionChecker folderPermissionChecker;

        public HealthCheckService(IDatabaseHealthCheck databaseHealthCheck,
            IEventStoreHealthCheck eventStoreHealthCheck, 
            INumberOfUnhandledPackagesChecker numberOfUnhandledPackagesChecker,
            INumberOfSyncPackagesWithBigSizeChecker numberOfSyncPackagesWithBigSizeChecker, 
            IFolderPermissionChecker folderPermissionChecker)
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

        private HealthCheckStatus GetGlobalStatus(ConnectionHealthCheckResult databaseHealthCheckResult, 
            ConnectionHealthCheckResult eventStoreHealthCheckResult, 
            NumberHealthCheckResult numberOfUnhandledPackages, 
            NumberHealthCheckResult numberOfSyncPackagesWithBigSize, 
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