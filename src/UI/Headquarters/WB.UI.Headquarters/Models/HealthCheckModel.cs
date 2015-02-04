using WB.Core.Infrastructure.HealthCheck;
using WB.Core.Infrastructure.Implementation.ReadSide;

namespace WB.UI.Headquarters.Models
{
    public class HealthCheckModel
    {
/*          check db, eventstore connection
            check if there are broken sync packages
            check if there are sync packages > 2mb
            check permissions on folders where site writes to
            check if there there was at least one denormalizer fail */

        public ConnectionHealthCheckResult DatabaseConnectionStatus { get; set; }
        public ConnectionHealthCheckResult EventstoreConnectionStatus { get; set; }
        public ReadSideStatus ReadSideServiceStatus { get; set; }

        public HealthCheckStatus Status { get; set; }
    }
}