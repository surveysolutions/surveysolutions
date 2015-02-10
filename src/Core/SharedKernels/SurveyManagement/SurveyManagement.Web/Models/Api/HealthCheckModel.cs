using System.Collections.Generic;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.HealthCheck;
using WB.Core.Infrastructure.Implementation.ReadSide;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models.Api
{
    public class HealthCheckModel
    {
        public HealthCheckModel(ConnectionHealthCheckResult databaseConnectionStatus,
            ConnectionHealthCheckResult eventstoreConnectionStatus, NumberHealthCheckResult numberOfUnhandledPackages,
            NumberHealthCheckResult numberOfSyncPackagesWithBigSize, FolderPermissionCheckResult folderPermissionCheckResult,
            ReadSideStatus readSideServiceStatus)
        {
            DatabaseConnectionStatus = databaseConnectionStatus;
            EventstoreConnectionStatus = eventstoreConnectionStatus;
            NumberOfUnhandledPackages = numberOfUnhandledPackages;
            NumberOfSyncPackagesWithBigSize = numberOfSyncPackagesWithBigSize;
            FolderPermissionCheckResult = folderPermissionCheckResult;
            ReadSideServiceStatus = readSideServiceStatus;
        }

        public ConnectionHealthCheckResult DatabaseConnectionStatus { get; private set; }
        public ConnectionHealthCheckResult EventstoreConnectionStatus { get; private set; }
        public ReadSideStatus ReadSideServiceStatus { get; private set; }
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