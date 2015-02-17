using System;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.HealthCheck.Checks
{
    class NumberOfSyncPackagesWithBigSizeChecker : IAtomicHealthCheck<NumberOfSyncPackagesWithBigSizeCheckResult>
    {
        private readonly IChunkReader chunkReader;

        public NumberOfSyncPackagesWithBigSizeChecker(IChunkReader chunkReader)
        {
            this.chunkReader = chunkReader;
        }

        public NumberOfSyncPackagesWithBigSizeCheckResult Check()
        {
            try
            {
                int count = chunkReader.GetNumberOfSyncPackagesWithBigSize();
                if (count == 0)
                    return NumberOfSyncPackagesWithBigSizeCheckResult.Happy(count);
                return NumberOfSyncPackagesWithBigSizeCheckResult.Warning(count,
                    "Some interviews are oversized. Please contact Survey Solutions Team <a href='support@mysurvey.solutions'>support@mysurvey.solutions</a> to report the error.");
            }
            catch (Exception e)
            {
                return NumberOfSyncPackagesWithBigSizeCheckResult.Error("The information about sync packages with big size can't be collected. " + e.Message);
            }
        }
    }
}