using System;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.HealthCheck;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services.HealthCheck.Checks
{
    public class ReadSideHealthChecker : IAtomicHealthCheck<ReadSideHealthCheckResult>
    {
        private readonly IPostgresReadSideBootstraper postgresReadSideBootstraper;
        public ReadSideHealthChecker(IPostgresReadSideBootstraper postgresReadSideBootstraper)
        {
            this.postgresReadSideBootstraper = postgresReadSideBootstraper;
        }

        public ReadSideHealthCheckResult Check()
        {
            try
            {
                return this.postgresReadSideBootstraper.CheckDatabaseConnection() ? 
                    ReadSideHealthCheckResult.Happy() : 
                    ReadSideHealthCheckResult.Down("Read side database is not available.");
            }
            catch (Exception e)
            {
                return ReadSideHealthCheckResult.Down("Read side database is not available. " + e.Message);
            }
        }
    }
}