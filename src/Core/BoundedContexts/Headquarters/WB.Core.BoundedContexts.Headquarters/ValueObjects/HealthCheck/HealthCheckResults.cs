using System.Text;

namespace WB.Core.BoundedContexts.Headquarters.ValueObjects.HealthCheck
{
    public class HealthCheckResults
    {
        public HealthCheckResults(HealthCheckStatus status,
            EventStoreHealthCheckResult eventstoreConnectionStatus, 
            NumberOfUnhandledPackagesHealthCheckResult numberOfUnhandledPackages,
            FolderPermissionCheckResult folderPermissionCheckResult,
            ReadSideHealthCheckResult readSideHealthCheckResult)
        {
            this.EventstoreConnectionStatus = eventstoreConnectionStatus;
            this.NumberOfUnhandledPackages = numberOfUnhandledPackages;
            this.FolderPermissionCheckResult = folderPermissionCheckResult;
            this.ReadSideHealthCheckResult = readSideHealthCheckResult;

            this.Status = status;
        }

        public EventStoreHealthCheckResult EventstoreConnectionStatus { get; private set; }
        public NumberOfUnhandledPackagesHealthCheckResult NumberOfUnhandledPackages { get; private set; }
        public FolderPermissionCheckResult FolderPermissionCheckResult { get; private set; }
        public ReadSideHealthCheckResult ReadSideHealthCheckResult { get; private set; }

        public HealthCheckStatus Status { get; private set; }

        public string GetStatusDescription()
        {
            var builder = new StringBuilder();
            builder.AppendLine(string.Format("EventStore: Status - {0} {1}", this.EventstoreConnectionStatus.Status, this.EventstoreConnectionStatus.ErrorMessage));
            builder.AppendLine(string.Format("UnhandledPackages: Status - {0} {1}", this.NumberOfUnhandledPackages.Status, this.NumberOfUnhandledPackages.ErrorMessage));
            builder.AppendLine(string.Format("FolderPermissions: Status - {0}", this.FolderPermissionCheckResult.Status));
            builder.AppendLine(string.Format("ReadSideHealth: Status - {0} {1}", this.ReadSideHealthCheckResult.Status, this.ReadSideHealthCheckResult.ErrorMessage));
            return builder.ToString();
        }
    }
}