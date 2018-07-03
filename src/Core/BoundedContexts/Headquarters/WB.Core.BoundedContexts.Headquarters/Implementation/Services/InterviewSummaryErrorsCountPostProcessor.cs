using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    /// <summary>
    /// Collect interview wide statistics
    /// </summary>
    public class InterviewSummaryErrorsCountPostProcessor : ICommandPostProcessor<StatefulInterview, InterviewCommand>
    {
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewSummaryReader;

        public InterviewSummaryErrorsCountPostProcessor(IReadSideRepositoryWriter<InterviewSummary> interviewSummaryReader)
        {
            this.interviewSummaryReader = interviewSummaryReader;
        }

        public void Process(StatefulInterview aggregate, InterviewCommand command)
        {
            var summary = interviewSummaryReader.GetById(aggregate.Id.FormatGuid());

            if (summary == null)
            {
                return;
            }

            if (!aggregate.HasErrors)
            {
                summary.ErrorsCount = 0;
            }
            else
            {
                var errorsCount = aggregate.CountAllInvalidEntities();
                summary.ErrorsCount = errorsCount;
            }

            interviewSummaryReader.Store(summary, aggregate.Id.FormatGuid());
        }
    }
}
