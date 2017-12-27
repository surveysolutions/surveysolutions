using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    internal class CumulativeChartDenormalizer : IFunctionalEventHandler, IEventHandler
    {
        internal class CumulativeState
        {
            public List<CumulativeReportStatusChange> Added { get; set; } = new List<CumulativeReportStatusChange>();
            public InterviewStatus? LastInterviewStatus { get; set; }
            public QuestionnaireIdentity QuestionnaireIdentity { get; set; }
            public bool IsDirty => Added.Any();
        }

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


        public void Handle(IEnumerable<IPublishableEvent> publishableEvents, Guid eventSourceId)
        {
            var lastStatusChangeEvent = publishableEvents.LastOrDefault(x => x.Payload is InterviewStatusChanged);

            if (lastStatusChangeEvent == null)
                return;

            var interviewStatusChanged = lastStatusChangeEvent.Payload as InterviewStatusChanged;

            var state = new CumulativeState
            {
                LastInterviewStatus = interviewStatusChanged?.PreviousStatus ?? cumulativeReportReader.Query(_ => _
                    .Where(x => x.InterviewId == eventSourceId && x.ChangeValue > 0)
                    .OrderByDescending(x => x.EventSequence)
                    .FirstOrDefault())?.Status,
                QuestionnaireIdentity = this.interviewReferencesStorage.GetQuestionnaireIdentity(eventSourceId)
            };

            this.Update(state, interviewStatusChanged, lastStatusChangeEvent);

            if (state.IsDirty)
                this.SaveState(state);
        }

        private void Update(CumulativeState state, InterviewStatusChanged statusCanged, IPublishableEvent publishableEvent)
        {
            InterviewStatus? oldStatus = state.LastInterviewStatus;

            InterviewStatus newStatus = statusCanged.Status;
            if (newStatus == InterviewStatus.Deleted)
                return;

            if (oldStatus != null)
            {
                var minusChange = new CumulativeReportStatusChange(
                    $"{publishableEvent.EventIdentifier.FormatGuid()}-minus",
                    state.QuestionnaireIdentity.QuestionnaireId,
                    state.QuestionnaireIdentity.Version,
                    publishableEvent.EventTimeStamp.Date, // time of synchronization
                    oldStatus.Value,
                    -1,
                    publishableEvent.EventSourceId,
                    publishableEvent.GlobalSequence);

                state.Added.Add(minusChange);
            }

            var plusChange = new CumulativeReportStatusChange(
                $"{publishableEvent.EventIdentifier.FormatGuid()}-plus",
                state.QuestionnaireIdentity.QuestionnaireId,
                state.QuestionnaireIdentity.Version,
                publishableEvent.EventTimeStamp.Date, // time of synchronization
                newStatus,
                +1,
                publishableEvent.EventSourceId,
                publishableEvent.GlobalSequence);

            state.Added.Add(plusChange);
        }

        protected void SaveState(CumulativeState state)
        {
            cumulativeReportStatusChangeStorage.BulkStore(state.Added.Select(x => new Tuple<CumulativeReportStatusChange, string>(x, x.EntryId)).ToList());
        }

        public void RegisterHandlersInOldFashionNcqrsBus(InProcessEventBus oldEventBus)
        {
            //no need in current implementation
        }

        public string Name => "Cumulative Chart Functional Denormalizer";
        public object[] Readers { get; } = new object[0];
        public object[] Writers { get; } = new object[0];
        
    }
}