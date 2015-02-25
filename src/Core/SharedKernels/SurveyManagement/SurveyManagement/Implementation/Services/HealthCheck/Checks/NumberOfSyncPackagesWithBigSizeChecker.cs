using System;
using System.Linq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck;
using WB.Core.Synchronization.Implementation.ReadSide.Indexes;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.HealthCheck.Checks
{
    internal class NumberOfSyncPackagesWithBigSizeChecker : IAtomicHealthCheck<NumberOfSyncPackagesWithBigSizeCheckResult>
    {
        private readonly IReadSideRepositoryIndexAccessor indexAccessor;

        private readonly string interviewQueryIndexName = typeof(SynchronizationDeltasByRecordSize).Name;

        public NumberOfSyncPackagesWithBigSizeChecker(IReadSideRepositoryIndexAccessor indexAccessor)
        {
            this.indexAccessor = indexAccessor;
        }

        public NumberOfSyncPackagesWithBigSizeCheckResult Check()
        {
            try
            {
                var groupedCount = indexAccessor.Query<SynchronizationDeltasByRecordSize.Result>(interviewQueryIndexName).FirstOrDefault();

                int count = groupedCount == null ? 0 : groupedCount.Count;

                if (count == 0) 
                    return NumberOfSyncPackagesWithBigSizeCheckResult.Happy(count);

                return NumberOfSyncPackagesWithBigSizeCheckResult.Warning(count, "Some interviews are oversized.<br />Please, contact Survey Solutions Team <a href='mailto:support@mysurvey.solutions'>support@mysurvey.solutions</a> to inform about the issue.");
            }
            catch (Exception e)
            {
                return NumberOfSyncPackagesWithBigSizeCheckResult.Error("The information about sync packages with big size can't be collected. " + e.Message);
            }
        }
    }
}