using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    internal class SurveyManagementInterviewCommandValidator : ICommandValidator<StatefulInterview, SynchronizeInterviewEventsCommand>,
        ICommandValidator<StatefulInterview, CreateInterview>
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryStorage;
        private readonly InterviewPreconditionsServiceSettings interviewPreconditionsServiceSettings;

        public SurveyManagementInterviewCommandValidator(
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryStorage, 
            InterviewPreconditionsServiceSettings interviewPreconditionsServiceSettings)
        {
            this.interviewSummaryStorage = interviewSummaryStorage;
            this.interviewPreconditionsServiceSettings = interviewPreconditionsServiceSettings;
        }


        private int? GetMaxAllowedInterviewsCount()
        {
            return this.interviewPreconditionsServiceSettings.InterviewLimitCount;
        }

        private int? GetInterviewsCountAllowedToCreateUntilLimitReached()
        {
            if (!this.interviewPreconditionsServiceSettings.InterviewLimitCount.HasValue)
                return null;

            var interviewsCountAllowedToCreateUntilLimitReached =
                this.interviewPreconditionsServiceSettings.InterviewLimitCount -
                this.QueryInterviewsCount();

            return interviewsCountAllowedToCreateUntilLimitReached;
        }

        private int QueryInterviewsCount()
        {
            return this.interviewSummaryStorage.Query(_ => _.Select(i => i.InterviewId).Count());
        }

        public void Validate(StatefulInterview aggregate, SynchronizeInterviewEventsCommand command)
        {
            if (command.CreatedOnClient)
            {
                this.ThrowIfInterviewCountLimitReached();
            }
        }


        private void ThrowIfInterviewCountLimitReached()
        {
            if (this.GetInterviewsCountAllowedToCreateUntilLimitReached() <= 0)
                throw new InterviewException(string.Format(SurveyManagementInterviewCommandValidatorMessages.LimitIsReachedErrorMessageFormat,
                    this.GetMaxAllowedInterviewsCount()),
                    InterviewDomainExceptionType.InterviewLimitReached);
        }

        public void Validate(StatefulInterview aggregate, CreateInterview command)
        {
            this.ThrowIfInterviewCountLimitReached();
        }
    }
}
