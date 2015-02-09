using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.HealthCheck;
using WB.Core.Infrastructure.Implementation.ReadSide;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models.Api
{
    public class HealthCheckModel
    {
        public HealthCheckModel(ConnectionHealthCheckResult databaseConnectionStatus, 
            ConnectionHealthCheckResult eventstoreConnectionStatus, int numberOfUnhandledPackages, 
            int numberOfSyncPackagesWithBigSize, FolderPermissionCheckResult folderPermissionCheckResult,
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
        public int NumberOfUnhandledPackages { get; private set; }
        public int NumberOfSyncPackagesWithBigSize { get; private set; }
        public FolderPermissionCheckResult FolderPermissionCheckResult { get; private set; }


        public HealthCheckStatus Status
        {
            get
            {
                var status = HealthCheckStatus.Happy;

                if (DatabaseConnectionStatus.Status != HealthCheckStatus.Happy
                    || EventstoreConnectionStatus.Status != HealthCheckStatus.Happy
                    || FolderPermissionCheckResult.DenidedFolders.Length > 0)
                {
                    status = HealthCheckStatus.Down;
                }
                else if (NumberOfUnhandledPackages > 0 || NumberOfSyncPackagesWithBigSize > 0)
                {
                    status = HealthCheckStatus.Warning;
                }

                return status;
            }
        }
    }
}