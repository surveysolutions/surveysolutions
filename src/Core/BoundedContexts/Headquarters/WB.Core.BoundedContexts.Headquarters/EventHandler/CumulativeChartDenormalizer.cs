using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    public class CumulativeChartDenormalizer : BaseDenormalizer, 
        IEventHandler<InterviewStatusChanged>,
        IEventHandler<InterviewHardDeleted>
    {
        private readonly IReadSideKeyValueStorage<LastInterviewStatus> lastStatusesStorage;
        private readonly IReadSideRepositoryWriter<CumulativeReportStatusChange> cumulativeReportStatusChangeStorage;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewReferencesStorage;

        public CumulativeChartDenormalizer(
            IReadSideKeyValueStorage<LastInterviewStatus> lastStatusesStorage,
            IReadSideRepositoryWriter<CumulativeReportStatusChange> cumulativeReportStatusChangeStorage,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewReferencesStorage)
        {
            this.lastStatusesStorage = lastStatusesStorage;
            this.cumulativeReportStatusChangeStorage = cumulativeReportStatusChangeStorage;
            this.interviewReferencesStorage = interviewReferencesStorage;
        }

        public override object[] Readers => new object[] { this.interviewReferencesStorage };

        public override object[] Writers => new object[] { this.lastStatusesStorage, this.cumulativeReportStatusChangeStorage };

        public void Handle(IPublishedEvent<InterviewStatusChanged> @event)
        {
            string interviewId = @event.EventSourceId.FormatGuid();

            InterviewStatus? oldStatus = this.lastStatusesStorage.GetById(interviewId)?.Status;
            InterviewStatus newStatus = @event.Payload.Status;

            var questionnaireIdentity = this.interviewReferencesStorage.GetQuestionnaireIdentity(@event.EventSourceId);

            var lastInterviewStatus = new LastInterviewStatus(interviewId, newStatus);
            this.lastStatusesStorage.Store(lastInterviewStatus, lastInterviewStatus.EntryId);

            if (oldStatus != null)
            {
                var minusChange = new CumulativeReportStatusChange(
                    $"{@event.EventIdentifier.FormatGuid()}-minus",
                    questionnaireIdentity.QuestionnaireId,
                    questionnaireIdentity.Version,
                    @event.EventTimeStamp.Date,
                    oldStatus.Value,
                    -1);

                this.cumulativeReportStatusChangeStorage.Store(minusChange, minusChange.EntryId);
            }

            var plusChange = new CumulativeReportStatusChange(
                $"{@event.EventIdentifier.FormatGuid()}-plus",
                questionnaireIdentity.QuestionnaireId,
                questionnaireIdentity.Version,
                @event.EventTimeStamp.Date,
                newStatus,
                +1);

            this.cumulativeReportStatusChangeStorage.Store(plusChange, plusChange.EntryId);
        }

        public void Handle(IPublishedEvent<InterviewHardDeleted> evnt)
        {
            this.lastStatusesStorage.Remove(evnt.EventSourceId);
        }
    }
}