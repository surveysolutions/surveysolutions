using System;
using System.Linq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.HealthCheck.Checks
{
    internal class NumberOfSyncPackagesWithBigSizeChecker : IAtomicHealthCheck<NumberOfSyncPackagesWithBigSizeCheckResult>
    {
        private const int WarningLength = 2097152;

        private readonly IQueryableReadSideRepositoryReader<InterviewSyncPackageMeta> interviewSyncPackes;
        private readonly IQueryableReadSideRepositoryReader<QuestionnaireSyncPackageMeta> questionnaireSyncPackages;

        public NumberOfSyncPackagesWithBigSizeChecker(IQueryableReadSideRepositoryReader<InterviewSyncPackageMeta> interviewSyncPackes,
            IQueryableReadSideRepositoryReader<QuestionnaireSyncPackageMeta> questionnaireSyncPackages)
        {
            this.interviewSyncPackes = interviewSyncPackes;
            this.questionnaireSyncPackages = questionnaireSyncPackages;
        }

        public NumberOfSyncPackagesWithBigSizeCheckResult Check()
        {
            try
            {
                var bigInterviewsPackages = this.interviewSyncPackes.Query(_ => _.Count(x => (x.ContentSize + x.MetaInfoSize) > WarningLength));
                var bigQuestionnaire = this.questionnaireSyncPackages.Query(_ => _.Count(x => (x.ContentSize + x.MetaInfoSize) > WarningLength));

                if (bigInterviewsPackages + bigQuestionnaire == 0) 
                    return NumberOfSyncPackagesWithBigSizeCheckResult.Happy(0);

                return NumberOfSyncPackagesWithBigSizeCheckResult.Warning(bigInterviewsPackages + bigQuestionnaire, "Some interviews are oversized.<br />Please, contact Survey Solutions Team <a href='mailto:support@mysurvey.solutions'>support@mysurvey.solutions</a> to inform about the issue.");
            }
            catch (Exception e)
            {
                return NumberOfSyncPackagesWithBigSizeCheckResult.Error("The information about sync packages with big size can't be collected. " + e.Message);
            }
        }
    }
}