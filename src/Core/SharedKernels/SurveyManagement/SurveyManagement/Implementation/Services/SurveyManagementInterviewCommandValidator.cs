using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using Microsoft.Practices.ServiceLocation;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services
{
    internal class SurveyManagementInterviewCommandValidator : ICommandValidator<Interview, SynchronizeInterviewEventsCommand>,
        ICommandValidator<Interview, CreateInterviewWithPreloadedData>,
        ICommandValidator<Interview, CreateInterviewCommand>,
        ICommandValidator<Interview, CreateInterviewOnClientCommand>,
        ICommandValidator<Interview, CreateInterviewCreatedOnClientCommand>
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryStorage;
        private readonly InterviewPreconditionsServiceSettings interviewPreconditionsServiceSettings;

        private ITransactionManagerProvider TransactionManagerProvider
        {
            get { return ServiceLocator.Current.GetInstance<ITransactionManagerProvider>(); }
        }

        public SurveyManagementInterviewCommandValidator(
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryStorage, 
            InterviewPreconditionsServiceSettings interviewPreconditionsServiceSettings)
        {
            this.interviewSummaryStorage = interviewSummaryStorage;
            this.interviewPreconditionsServiceSettings = interviewPreconditionsServiceSettings;
        }

        private int? GetMaxAllowedInterviewsCount()
        {
            return interviewPreconditionsServiceSettings.InterviewLimitCount;
        }

        private int? GetInterviewsCountAllowedToCreateUntilLimitReached()
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

        public void Validate(Interview aggregate, SynchronizeInterviewEventsCommand command)
        {
            if (command.CreatedOnClient)
            {
                this.ThrowIfInterviewCountLimitReached();
            }
        }


        private void ThrowIfInterviewCountLimitReached()
        {
            if (GetInterviewsCountAllowedToCreateUntilLimitReached() <= 0)
                throw new InterviewException(string.Format("Max number of interviews '{0}' is reached.",
                    GetMaxAllowedInterviewsCount()),
                    InterviewDomainExceptionType.InterviewLimitReached);
        }

        public void Validate(Interview aggregate, CreateInterviewWithPreloadedData command)
        {
            this.ThrowIfInterviewCountLimitReached();
        }

        public void Validate(Interview aggregate, CreateInterviewCommand command)
        {
            this.ThrowIfInterviewCountLimitReached();
        }

        public void Validate(Interview aggregate, CreateInterviewOnClientCommand command)
        {
            this.ThrowIfInterviewCountLimitReached();
        }

        public void Validate(Interview aggregate, CreateInterviewCreatedOnClientCommand command)
        {
            this.ThrowIfInterviewCountLimitReached();
        }
    }
}
