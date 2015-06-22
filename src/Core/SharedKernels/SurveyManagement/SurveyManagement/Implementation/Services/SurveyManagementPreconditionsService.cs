using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using Microsoft.Practices.ServiceLocation;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services
{
    internal class SurveyManagementPreconditionsService : IInterviewPreconditionsService
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryStorage;
        private readonly InterviewPreconditionsServiceSettings interviewPreconditionsServiceSettings;

        private ITransactionManagerProvider TransactionManagerProvider
        {
            get { return ServiceLocator.Current.GetInstance<ITransactionManagerProvider>(); }
        }

        public SurveyManagementPreconditionsService(
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryStorage, 
            InterviewPreconditionsServiceSettings interviewPreconditionsServiceSettings)
        {
            this.interviewSummaryStorage = interviewSummaryStorage;
            this.interviewPreconditionsServiceSettings = interviewPreconditionsServiceSettings;
        }

        public int? GetMaxAllowedInterviewsCount()
        {
            return interviewPreconditionsServiceSettings.InterviewLimitCount;
        }

        public int? GetInterviewsCountAllowedToCreateUntilLimitReached()
        {
            if (!interviewPreconditionsServiceSettings.InterviewLimitCount.HasValue)
                return null;

            var interviewsCountAllowedToCreateUntilLimitReached =
                interviewPreconditionsServiceSettings.InterviewLimitCount -
                QueryInterviewsCount();

            return interviewsCountAllowedToCreateUntilLimitReached;
        }

        private int QueryInterviewsCount()
        {
            var shouldUseOwnTransaction = !TransactionManagerProvider.GetTransactionManager().IsQueryTransactionStarted;

            if (shouldUseOwnTransaction)
            {
                this.TransactionManagerProvider.GetTransactionManager().BeginQueryTransaction();
            }
            try
            {
                return interviewSummaryStorage.Query(_ => _.Select(i => i.InterviewId).Count());
            }
            finally
            {
                if (shouldUseOwnTransaction)
                {
                    this.TransactionManagerProvider.GetTransactionManager().RollbackQueryTransaction();
                }
            }
        }
    }

    internal class InterviewPreconditionsServiceSettings
    {
        public InterviewPreconditionsServiceSettings(int? interviewLimitCount)
        {
            InterviewLimitCount = interviewLimitCount;
        }

        public int? InterviewLimitCount { get; private set; }
    }
}
