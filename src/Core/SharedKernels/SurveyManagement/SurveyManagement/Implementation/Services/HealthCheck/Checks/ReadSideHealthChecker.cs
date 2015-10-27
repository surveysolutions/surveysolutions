using System;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.HealthCheck.Checks
{
    public class ReadSideHealthChecker : IAtomicHealthCheck<ReadSideHealthCheckResult>
    {
        private readonly IPostgresReadSideBootstraper readSideCheckerAndCleaner;
        public ReadSideHealthChecker(IPostgresReadSideBootstraper readSideCheckerAndCleaner)
        {
            this.readSideCheckerAndCleaner = readSideCheckerAndCleaner;
        }

        public ReadSideHealthCheckResult Check()
        {
            try
            {
                return readSideCheckerAndCleaner.CheckDatabaseConnection() ? 
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