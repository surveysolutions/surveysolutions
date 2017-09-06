using System.Linq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    public class CumulativeChartDenormalizer : BaseDenormalizer, 
        IEventHandler<InterviewStatusChanged>
    {
        private readonly INativeReadSideStorage<CumulativeReportStatusChange> cumulativeReportReader;
        private readonly IReadSideRepositoryWriter<CumulativeReportStatusChange> cumulativeReportStatusChangeStorage;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewReferencesStorage;

        public CumulativeChartDenormalizer(
            IReadSideRepositoryWriter<CumulativeReportStatusChange> cumulativeReportStatusChangeStorage,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewReferencesStorage,
            INativeReadSideStorage<CumulativeReportStatusChange> cumulativeReportReader)
        {
            this.cumulativeReportStatusChangeStorage = cumulativeReportStatusChangeStorage;
            this.interviewReferencesStorage = interviewReferencesStorage;
            this.cumulativeReportReader = cumulativeReportReader;
        }

        public override object[] Readers => new object[] { this.interviewReferencesStorage };

        public override object[] Writers => new object[] { this.cumulativeReportStatusChangeStorage };

        public void Handle(IPublishedEvent<InterviewStatusChanged> @event)
        {
            InterviewStatus? oldStatus = cumulativeReportReader.Query(_ => _
                .Where(x => x.InterviewId == @event.EventSourceId && x.ChangeValue > 0)
                .OrderByDescending(x => x.EventSequence)
                .FirstOrDefault())?.Status;

            InterviewStatus newStatus = @event.Payload.Status;

            var questionnaireIdentity = this.interviewReferencesStorage.GetQuestionnaireIdentity(@event.EventSourceId);

            if (oldStatus != null)
            {
                var minusChange = new CumulativeReportStatusChange(
                    $"{@event.EventIdentifier.FormatGuid()}-minus",
                    questionnaireIdentity.QuestionnaireId,
                    questionnaireIdentity.Version,
                    @event.EventTimeStamp.Date,
                    oldStatus.Value,
                    -1,
                    @event.EventSourceId,
                    @event.GlobalSequence);

                this.cumulativeReportStatusChangeStorage.Store(minusChange, minusChange.EntryId);
            }

            var plusChange = new CumulativeReportStatusChange(
                $"{@event.EventIdentifier.FormatGuid()}-plus",
                questionnaireIdentity.QuestionnaireId,
                questionnaireIdentity.Version,
                @event.EventTimeStamp.Date,
                newStatus,
                +1,
                @event.EventSourceId,
                @event.GlobalSequence);

            this.cumulativeReportStatusChangeStorage.Store(plusChange, plusChange.EntryId);
        }
    }
}