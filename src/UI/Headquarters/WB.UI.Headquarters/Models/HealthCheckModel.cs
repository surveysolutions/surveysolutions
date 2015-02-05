using WB.Core.Infrastructure.HealthCheck;
using WB.Core.Infrastructure.Implementation.ReadSide;

namespace WB.UI.Headquarters.Models
{
    public class HealthCheckModel
    {
        public ConnectionHealthCheckResult DatabaseConnectionStatus { get; set; }
        public ConnectionHealthCheckResult EventstoreConnectionStatus { get; set; }
        public ReadSideStatus ReadSideServiceStatus { get; set; }
        public int NumberOfUnhandledPackages { get; set; }
        public int NumberOfSyncPackagesWithBigSize { get; set; }

        public HealthCheckStatus Status { get; set; }
    }
}