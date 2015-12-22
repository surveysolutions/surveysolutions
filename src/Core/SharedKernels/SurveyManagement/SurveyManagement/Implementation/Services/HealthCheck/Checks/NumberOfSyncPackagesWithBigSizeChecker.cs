using System;
using System.Linq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.HealthCheck.Checks
{
    internal class NumberOfSyncPackagesWithBigSizeChecker : IAtomicHealthCheck<NumberOfSyncPackagesWithBigSizeCheckResult>
    {
        private const int WarningLength = 2097152;

        private readonly ITransactionManagerProvider transactionManagerProvider;
        private readonly IQueryableReadSideRepositoryReader<InterviewSyncPackageMeta> interviewSyncPackes;
        private readonly IQueryableReadSideRepositoryReader<QuestionnaireBrowseItem> questionnairesRepository;

        public NumberOfSyncPackagesWithBigSizeChecker(IQueryableReadSideRepositoryReader<InterviewSyncPackageMeta> interviewSyncPackes,
            IQueryableReadSideRepositoryReader<QuestionnaireBrowseItem> questionnairesRepository,
            ITransactionManagerProvider transactionManagerProvider)
        {
            this.interviewSyncPackes = interviewSyncPackes;
            this.questionnairesRepository = questionnairesRepository;
            this.transactionManagerProvider = transactionManagerProvider;
        }

        public NumberOfSyncPackagesWithBigSizeCheckResult Check()
        {
            var shouldUseOwnTransaction = !this.transactionManagerProvider.GetTransactionManager().IsQueryTransactionStarted;
            try
            {
                if (shouldUseOwnTransaction)
                {
                    this.transactionManagerProvider.GetTransactionManager().BeginQueryTransaction();
                }
                var bigInterviewsPackages = this.interviewSyncPackes.Query(_ => _.Count(x => x.SerializedPackageSize > WarningLength));
                var bigQuestionnaire = this.questionnairesRepository.Query(_ => _.Count(x => x.SerializedQuestionnaireSize > WarningLength));

                if (bigInterviewsPackages + bigQuestionnaire == 0)
                    return NumberOfSyncPackagesWithBigSizeCheckResult.Happy(0);

                return NumberOfSyncPackagesWithBigSizeCheckResult.Warning(bigInterviewsPackages + bigQuestionnaire,
                    "Some interviews are oversized.<br />Please, contact Survey Solutions Team <a href='mailto:support@mysurvey.solutions'>support@mysurvey.solutions</a> to inform about the issue.");
            }
            catch (Exception e)
            {
                return
                    NumberOfSyncPackagesWithBigSizeCheckResult.Error(
                        "The information about sync packages with big size can't be collected. " + e.Message);
            }
            finally
            {
                if (shouldUseOwnTransaction)
                {
                    this.transactionManagerProvider.GetTransactionManager().RollbackQueryTransaction();
                }
            }
        }
    }
}