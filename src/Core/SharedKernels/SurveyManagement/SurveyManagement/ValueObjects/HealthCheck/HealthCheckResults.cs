using System.Text;

namespace WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck
{
    public class HealthCheckResults
    {
        public HealthCheckResults(HealthCheckStatus status,
            EventStoreHealthCheckResult eventstoreConnectionStatus, 
            NumberOfUnhandledPackagesHealthCheckResult numberOfUnhandledPackages,
            FolderPermissionCheckResult folderPermissionCheckResult,
            ReadSideHealthCheckResult readSideHealthCheckResult)
        {
            EventstoreConnectionStatus = eventstoreConnectionStatus;
            NumberOfUnhandledPackages = numberOfUnhandledPackages;
            FolderPermissionCheckResult = folderPermissionCheckResult;
            ReadSideHealthCheckResult = readSideHealthCheckResult;

            Status = status;
        }

        public EventStoreHealthCheckResult EventstoreConnectionStatus { get; private set; }
        public NumberOfUnhandledPackagesHealthCheckResult NumberOfUnhandledPackages { get; private set; }
        public FolderPermissionCheckResult FolderPermissionCheckResult { get; private set; }
        public ReadSideHealthCheckResult ReadSideHealthCheckResult { get; private set; }

        public HealthCheckStatus Status { get; private set; }

        public string GetStatusDescription()
        {
            var builder = new StringBuilder();
            builder.AppendLine(string.Format("EventStore: Status - {0} {1}", EventstoreConnectionStatus.Status, EventstoreConnectionStatus.ErrorMessage));
            builder.AppendLine(string.Format("UnhandledPackages: Status - {0} {1}", NumberOfUnhandledPackages.Status, NumberOfUnhandledPackages.ErrorMessage));
            builder.AppendLine(string.Format("FolderPermissions: Status - {0}", FolderPermissionCheckResult.Status));
            builder.AppendLine(string.Format("ReadSideHealth: Status - {0} {1}", ReadSideHealthCheckResult.Status, ReadSideHealthCheckResult.ErrorMessage));
            return builder.ToString();
        }
    }
}