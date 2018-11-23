using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    internal class InterviewSummaryCompositeDenormalizer : 
        AbstractCompositeFunctionalEventHandler<InterviewSummary, IReadSideRepositoryWriter<InterviewSummary>>
    {
        public InterviewSummaryCompositeDenormalizer(
            IReadSideRepositoryWriter<InterviewSummary> readSideStorage,
            InterviewSummaryDenormalizer interviewSummaryDenormalizer,
            StatusChangeHistoryDenormalizerFunctional historyDenormalizerFunctional,
            InterviewStatusTimeSpanDenormalizer statusTimeSpanDenormalizer) : base(readSideStorage)
        {
            Handlers = new ICompositeFunctionalPartEventHandler<InterviewSummary, IReadSideRepositoryWriter<InterviewSummary>>[]
            {
                interviewSummaryDenormalizer,
                historyDenormalizerFunctional,
                statusTimeSpanDenormalizer
            };
        }

        public override ICompositeFunctionalPartEventHandler<InterviewSummary, IReadSideRepositoryWriter<InterviewSummary>>[] Handlers
        {
            get;
        }

        public override object[] Readers => new object[0];

    }
}
