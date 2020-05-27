using Microsoft.Extensions.Caching.Memory;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Infrastructure.Native.Storage;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    internal class InterviewSummaryCompositeDenormalizer :
        AbstractCompositeFunctionalEventHandler<InterviewSummary, IReadSideRepositoryWriter<InterviewSummary>>
    {
        public InterviewSummaryCompositeDenormalizer(
            IReadSideRepositoryWriter<InterviewSummary> readSideStorage,
            InterviewSummaryDenormalizer interviewSummaryDenormalizer,
            StatusChangeHistoryDenormalizerFunctional historyDenormalizerFunctional,
            InterviewStatusTimeSpanDenormalizer statusTimeSpanDenormalizer,
            IInterviewStatisticsReportDenormalizer statisticsReportDenormalizer,
            InterviewGeoLocationAnswersDenormalizer geoLocationAnswersDenormalizer,
            InterviewExportedCommentariesDenormalizer commentsDenormalizer) : base(readSideStorage)
        {
            Handlers = new ICompositeFunctionalPartEventHandler<InterviewSummary, IReadSideRepositoryWriter<InterviewSummary>>[]
            {
                interviewSummaryDenormalizer,
                historyDenormalizerFunctional,
                statusTimeSpanDenormalizer,
                geoLocationAnswersDenormalizer,
                statisticsReportDenormalizer,
                commentsDenormalizer
            };
        }

        public override ICompositeFunctionalPartEventHandler<InterviewSummary, IReadSideRepositoryWriter<InterviewSummary>>[] Handlers
        {
            get;
        }
    }
}
