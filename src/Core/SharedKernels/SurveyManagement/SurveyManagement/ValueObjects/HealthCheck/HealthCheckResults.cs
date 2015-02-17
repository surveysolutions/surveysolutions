using WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck;

namespace WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck
{
    public class HealthCheckResults
    {
        public HealthCheckResults(HealthCheckStatus status, 
            ConnectionHealthCheckResult databaseConnectionStatus,
            ConnectionHealthCheckResult eventstoreConnectionStatus, 
            NumberHealthCheckResult numberOfUnhandledPackages,
            NumberHealthCheckResult numberOfSyncPackagesWithBigSize, 
            FolderPermissionCheckResult folderPermissionCheckResult)
        {
            DatabaseConnectionStatus = databaseConnectionStatus;
            EventstoreConnectionStatus = eventstoreConnectionStatus;
            NumberOfUnhandledPackages = numberOfUnhandledPackages;
            NumberOfSyncPackagesWithBigSize = numberOfSyncPackagesWithBigSize;
            FolderPermissionCheckResult = folderPermissionCheckResult;

            Status = status;
        }

        public ConnectionHealthCheckResult DatabaseConnectionStatus { get; private set; }
        public ConnectionHealthCheckResult EventstoreConnectionStatus { get; private set; }
        public NumberHealthCheckResult NumberOfUnhandledPackages { get; private set; }
        public NumberHealthCheckResult NumberOfSyncPackagesWithBigSize { get; private set; }
        public FolderPermissionCheckResult FolderPermissionCheckResult { get; private set; }


        public HealthCheckStatus Status { get; private set; }
    }
}