using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.HealthCheck;
using WB.Core.Infrastructure.Implementation.ReadSide;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class HealthCheckModel
    {
        public ConnectionHealthCheckResult DatabaseConnectionStatus { get; set; }
        public ConnectionHealthCheckResult EventstoreConnectionStatus { get; set; }
        public ReadSideStatus ReadSideServiceStatus { get; set; }
        public int NumberOfUnhandledPackages { get; set; }
        public int NumberOfSyncPackagesWithBigSize { get; set; }
        public FolderPermissionCheckResult FolderPermissionCheckResult { get; set; }

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