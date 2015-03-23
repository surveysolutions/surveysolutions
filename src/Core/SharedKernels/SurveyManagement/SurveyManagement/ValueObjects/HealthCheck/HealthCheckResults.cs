using WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck;

namespace WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck
{
    public class HealthCheckResults
    {
        public HealthCheckResults(HealthCheckStatus status,
            RavenHealthCheckResult databaseConnectionStatus,
            EventStoreHealthCheckResult eventstoreConnectionStatus, 
            NumberOfUnhandledPackagesHealthCheckResult numberOfUnhandledPackages,
            NumberOfSyncPackagesWithBigSizeCheckResult numberOfSyncPackagesWithBigSize, 
            FolderPermissionCheckResult folderPermissionCheckResult)
        {
            DatabaseConnectionStatus = databaseConnectionStatus;
            EventstoreConnectionStatus = eventstoreConnectionStatus;
            NumberOfUnhandledPackages = numberOfUnhandledPackages;
            NumberOfSyncPackagesWithBigSize = numberOfSyncPackagesWithBigSize;
            FolderPermissionCheckResult = folderPermissionCheckResult;

            Status = status;
        }

        public RavenHealthCheckResult DatabaseConnectionStatus { get; private set; }
        public EventStoreHealthCheckResult EventstoreConnectionStatus { get; private set; }
        public NumberOfUnhandledPackagesHealthCheckResult NumberOfUnhandledPackages { get; private set; }
        public NumberOfSyncPackagesWithBigSizeCheckResult NumberOfSyncPackagesWithBigSize { get; private set; }
        public FolderPermissionCheckResult FolderPermissionCheckResult { get; private set; }


        public HealthCheckStatus Status { get; private set; }
    }
}