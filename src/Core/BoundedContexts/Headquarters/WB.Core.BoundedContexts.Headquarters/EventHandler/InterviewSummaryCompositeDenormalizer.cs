using System;
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
        private readonly IMemoryCache memoryCache;
        private readonly INativeReadSideStorage<InterviewSummary, int> summaries;

        public InterviewSummaryCompositeDenormalizer(
            IReadSideRepositoryWriter<InterviewSummary> readSideStorage,
            INativeReadSideStorage<InterviewSummary, int> summaries,
            InterviewSummaryDenormalizer interviewSummaryDenormalizer,
            StatusChangeHistoryDenormalizerFunctional historyDenormalizerFunctional,
            InterviewStatusTimeSpanDenormalizer statusTimeSpanDenormalizer,
            IInterviewStatisticsReportDenormalizer statisticsReportDenormalizer, 
            InterviewGeoLocationAnswersDenormalizer geoLocationAnswersDenormalizer, 
            IMemoryCache memoryCache) : base(readSideStorage)
        {
            this.summaries = summaries;
            this.memoryCache = memoryCache;
            Handlers = new ICompositeFunctionalPartEventHandler<InterviewSummary, IReadSideRepositoryWriter<InterviewSummary>>[]
            {
                interviewSummaryDenormalizer,
                historyDenormalizerFunctional,
                statusTimeSpanDenormalizer,
                geoLocationAnswersDenormalizer,
                statisticsReportDenormalizer
            };
        }

        protected override InterviewSummary GetViewById(Guid id, IReadSideStorage<InterviewSummary> storage)
        {

            var cachedId = this.memoryCache.Get<int>(id);
            if (cachedId != 0)
            {
                return this.summaries.GetById(cachedId);
            }

            var viewById = base.GetViewById(id, storage);
            if (viewById != null)
            {
                this.memoryCache.Set(id, viewById.Id, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(10)
                });
            }

            return viewById;
        }

        public override ICompositeFunctionalPartEventHandler<InterviewSummary, IReadSideRepositoryWriter<InterviewSummary>>[] Handlers
        {
            get;
        }

        public override object[] Readers { get; } = Array.Empty<object>();
    }
}
