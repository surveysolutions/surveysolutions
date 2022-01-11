using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    internal class InterviewSummaryCompositeDenormalizer :
        AbstractCompositeFunctionalEventHandler<InterviewSummary, IReadSideRepositoryWriter<InterviewSummary>>
    {
        public InterviewSummaryCompositeDenormalizer(
            EventBusSettings eventBusSettings,
            IReadSideRepositoryWriter<InterviewSummary> readSideStorage,
            InterviewSummaryDenormalizer interviewSummaryDenormalizer,
            StatusChangeHistoryDenormalizerFunctional historyDenormalizerFunctional,
            InterviewStatusTimeSpanDenormalizer statusTimeSpanDenormalizer,
            IInterviewStatisticsReportDenormalizer statisticsReportDenormalizer,
            InterviewGeoLocationAnswersDenormalizer geoLocationAnswersDenormalizer,
            InterviewExportedCommentariesDenormalizer commentsDenormalizer,
            InterviewDynamicReportAnswersDenormalizer dynamicReportAnswersDenormalizer) : base(eventBusSettings, readSideStorage)
        {
            Handlers = new ICompositeFunctionalPartEventHandler<InterviewSummary, IReadSideRepositoryWriter<InterviewSummary>>[]
            {
                interviewSummaryDenormalizer,
                historyDenormalizerFunctional,
                statusTimeSpanDenormalizer,
                geoLocationAnswersDenormalizer,
                statisticsReportDenormalizer,
                commentsDenormalizer,
                dynamicReportAnswersDenormalizer
            };
        }

        public override ICompositeFunctionalPartEventHandler<InterviewSummary, IReadSideRepositoryWriter<InterviewSummary>>[] Handlers
        {
            get;
        }
    }
}
