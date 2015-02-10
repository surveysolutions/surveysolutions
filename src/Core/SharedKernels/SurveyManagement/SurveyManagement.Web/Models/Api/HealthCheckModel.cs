using System.Collections.Generic;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.HealthCheck;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models.Api
{
    public class HealthCheckModel
    {
        public HealthCheckModel(ConnectionHealthCheckResult databaseConnectionStatus,
            ConnectionHealthCheckResult eventstoreConnectionStatus, NumberHealthCheckResult numberOfUnhandledPackages,
            NumberHealthCheckResult numberOfSyncPackagesWithBigSize, FolderPermissionCheckResult folderPermissionCheckResult)
        {
            DatabaseConnectionStatus = databaseConnectionStatus;
            EventstoreConnectionStatus = eventstoreConnectionStatus;
            NumberOfUnhandledPackages = numberOfUnhandledPackages;
            NumberOfSyncPackagesWithBigSize = numberOfSyncPackagesWithBigSize;
            FolderPermissionCheckResult = folderPermissionCheckResult;
        }

        public ConnectionHealthCheckResult DatabaseConnectionStatus { get; private set; }
        public ConnectionHealthCheckResult EventstoreConnectionStatus { get; private set; }
        public NumberHealthCheckResult NumberOfUnhandledPackages { get; private set; }
        public NumberHealthCheckResult NumberOfSyncPackagesWithBigSize { get; private set; }
        public FolderPermissionCheckResult FolderPermissionCheckResult { get; private set; }


        public HealthCheckStatus Status
        {
            get
            {
                HashSet<HealthCheckStatus> statuses = new HashSet<HealthCheckStatus>(
                new [] {
                    DatabaseConnectionStatus.Status,
                    EventstoreConnectionStatus.Status,
                    FolderPermissionCheckResult.Status,
                    NumberOfUnhandledPackages.Status,
                    NumberOfSyncPackagesWithBigSize.Status
                });

                if (statuses.Contains(HealthCheckStatus.Down))
                    return HealthCheckStatus.Down;
                if (statuses.Contains(HealthCheckStatus.Warning))
                    return HealthCheckStatus.Warning;

                return HealthCheckStatus.Happy;
            }
        }
    }
}