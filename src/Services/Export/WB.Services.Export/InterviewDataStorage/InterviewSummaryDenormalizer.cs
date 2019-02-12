using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Events.Interview;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.InterviewDataStorage
{
    public class InterviewSummaryDenormalizer:
        IHighPriorityFunctionalHandler,
        IEventHandler<InterviewCreated>,
        IEventHandler<InterviewHardDeleted>,
        IEventHandler<InterviewStatusChanged>,
        IEventHandler<InterviewKeyAssigned>,
        IEventHandler<AnswersRemoved>,
        IEventHandler<TextQuestionAnswered>,
        IEventHandler<NumericIntegerQuestionAnswered>,
        IEventHandler<NumericRealQuestionAnswered>,
        IEventHandler<TextListQuestionAnswered>,
        IEventHandler<MultipleOptionsLinkedQuestionAnswered>,
        IEventHandler<MultipleOptionsQuestionAnswered>,
        IEventHandler<SingleOptionQuestionAnswered>,
        IEventHandler<SingleOptionLinkedQuestionAnswered>,
        IEventHandler<AreaQuestionAnswered>,
        IEventHandler<AudioQuestionAnswered>,
        IEventHandler<DateTimeQuestionAnswered>,
        IEventHandler<GeoLocationQuestionAnswered>,
        IEventHandler<PictureQuestionAnswered>,
        IEventHandler<QRBarcodeQuestionAnswered>,
        IEventHandler<YesNoQuestionAnswered>
    {
        private readonly ITenantContext tenantContext;

        public InterviewSummaryDenormalizer(ITenantContext tenantContext)
        {
            this.tenantContext = tenantContext;
        }

        private readonly Dictionary<Guid, InterviewSummary> added = new Dictionary<Guid, InterviewSummary>();
        private readonly Dictionary<Guid, List<Action<InterviewSummary>>> changes = new Dictionary<Guid, List<Action<InterviewSummary>>>();
        private readonly HashSet<Guid> removed = new HashSet<Guid>();

        public void Handle(PublishedEvent<InterviewCreated> @event)
        {
            added.Add(
                @event.EventSourceId,
                new InterviewSummary
                {
                    InterviewId = @event.EventSourceId,
                    QuestionnaireId = @event.Event.QuestionnaireIdentity,
                    Status = InterviewStatus.Created,
                    UpdateDateUtc = @event.EventTimeStamp
                });
        }

        public void Handle(PublishedEvent<InterviewHardDeleted> @event)
        {
            if (!added.Remove(@event.EventSourceId))
            {
                removed.Add(@event.EventSourceId);
            }
        }

        public void Handle(PublishedEvent<InterviewStatusChanged> @event)
        {
            EnlistChange(@event.EventSourceId, @event.EventTimeStamp, summary => summary.Status = @event.Event.Status);
        }

        public void Handle(PublishedEvent<InterviewKeyAssigned> @event)
        {
            EnlistChange(@event.EventSourceId, @event.EventTimeStamp, summary => summary.Key = @event.Event.Key.ToString());
        }

        public void Handle(PublishedEvent<AnswersRemoved> @event)
        {
            EnlistChange(@event.EventSourceId, @event.EventTimeStamp, null);
        }

        private void EnlistChange(Guid interviewId, DateTime updateDate, Action<InterviewSummary> action)
        {
            var changesList = this.changes.GetOrNull(interviewId) ?? new List<Action<InterviewSummary>>();
            if (action != null)
            {
                changesList.Add(action);
            }
            changesList.Add(summary => summary.UpdateDateUtc = updateDate);
            this.changes[interviewId] = changesList;
        }

        public async Task SaveStateAsync(CancellationToken cancellationToken = default)
        {
            var db = this.tenantContext.DbContext;

            db.InterviewReferences.AddRange(added.Values);

            var arg = new object[1];
            foreach (var remove in removed)
            {
                arg[0] = remove;
                var reference = await db.InterviewReferences.FindAsync(arg, cancellationToken);
                db.InterviewReferences.Remove(reference);
            }

            foreach (KeyValuePair<Guid, List<Action<InterviewSummary>>> change in changes)
            {
                var reference = db.InterviewReferences.Find(change.Key);
                if (reference != null)
                {
                    foreach (var action in change.Value)
                    {
                        action(reference);
                    }
                }
            }

            await db.SaveChangesAsync(cancellationToken);
        }

        public void Handle(PublishedEvent<TextQuestionAnswered> @event)
        {
            EnlistChange(@event.EventSourceId, @event.EventTimeStamp, null);
        }

        public void Handle(PublishedEvent<NumericIntegerQuestionAnswered> @event)
        {
            EnlistChange(@event.EventSourceId, @event.EventTimeStamp, null);
        }

        public void Handle(PublishedEvent<NumericRealQuestionAnswered> @event)
        {
            EnlistChange(@event.EventSourceId, @event.EventTimeStamp, null);
        }

        public void Handle(PublishedEvent<TextListQuestionAnswered> @event)
        {
            EnlistChange(@event.EventSourceId, @event.EventTimeStamp, null);
        }

        public void Handle(PublishedEvent<MultipleOptionsLinkedQuestionAnswered> @event)
        {
            EnlistChange(@event.EventSourceId, @event.EventTimeStamp, null);
        }

        public void Handle(PublishedEvent<MultipleOptionsQuestionAnswered> @event)
        {
            EnlistChange(@event.EventSourceId, @event.EventTimeStamp, null);
        }

        public void Handle(PublishedEvent<SingleOptionQuestionAnswered> @event)
        {
            EnlistChange(@event.EventSourceId, @event.EventTimeStamp, null);
        }

        public void Handle(PublishedEvent<SingleOptionLinkedQuestionAnswered> @event)
        {
            EnlistChange(@event.EventSourceId, @event.EventTimeStamp, null);
        }

        public void Handle(PublishedEvent<AreaQuestionAnswered> @event)
        {
            EnlistChange(@event.EventSourceId, @event.EventTimeStamp, null);
        }

        public void Handle(PublishedEvent<AudioQuestionAnswered> @event)
        {
            EnlistChange(@event.EventSourceId, @event.EventTimeStamp, null);
        }

        public void Handle(PublishedEvent<DateTimeQuestionAnswered> @event)
        {
            EnlistChange(@event.EventSourceId, @event.EventTimeStamp, null);
        }

        public void Handle(PublishedEvent<GeoLocationQuestionAnswered> @event)
        {
            EnlistChange(@event.EventSourceId, @event.EventTimeStamp, null);
        }

        public void Handle(PublishedEvent<PictureQuestionAnswered> @event)
        {
            EnlistChange(@event.EventSourceId, @event.EventTimeStamp, null);
        }

        public void Handle(PublishedEvent<QRBarcodeQuestionAnswered> @event)
        {
            EnlistChange(@event.EventSourceId, @event.EventTimeStamp, null);
        }

        public void Handle(PublishedEvent<YesNoQuestionAnswered> @event)
        {
            EnlistChange(@event.EventSourceId, @event.EventTimeStamp, null);
        }
    }
}
