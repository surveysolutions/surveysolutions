using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Events.Interview;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Infrastructure.EventSourcing;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.InterviewDataStorage
{
    public class InterviewSummaryDenormalizer:
        IFunctionalHandler,
        IEventHandler<InterviewCreated>,
        IEventHandler<InterviewOnClientCreated>,
        IEventHandler<InterviewFromPreloadedDataCreated>,
        IEventHandler<InterviewDeleted>,
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
        private readonly TenantDbContext dbContext;

        public InterviewSummaryDenormalizer(TenantDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        private readonly Dictionary<Guid, List<Action<InterviewReference>>> changes = new Dictionary<Guid, List<Action<InterviewReference>>>();

        public void Handle(PublishedEvent<InterviewCreated> @event)
        {
            EnlistChange(@event.EventSourceId, @event.EventTimeStamp, s=>
            {
                s.AssignmentId = @event.Event.AssignmentId;
                s.Status = InterviewStatus.Created;
            });
        }

        public void Handle(PublishedEvent<InterviewOnClientCreated> @event)
        {
            EnlistChange(@event.EventSourceId, @event.EventTimeStamp, s =>
            {
                s.AssignmentId = @event.Event.AssignmentId;
                s.Status = InterviewStatus.Created;
            });
        }

        public void Handle(PublishedEvent<InterviewFromPreloadedDataCreated> @event)
        {
            EnlistChange(@event.EventSourceId, @event.EventTimeStamp, s =>
            {
                s.AssignmentId = @event.Event.AssignmentId;
                s.Status = InterviewStatus.Created;
            });
        }

        public void Handle(PublishedEvent<InterviewDeleted> @event)
        {
            EnlistChange(@event.EventSourceId, @event.EventTimeStamp, summary => summary.Status = InterviewStatus.Deleted);
        }

        public void Handle(PublishedEvent<InterviewHardDeleted> @event)
        {
            EnlistChange(@event.EventSourceId, @event.EventTimeStamp, summary => summary.Status = InterviewStatus.Deleted);
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

        private void EnlistChange(Guid interviewId, DateTime updateDate, Action<InterviewReference>? action)
        {
            var changesList = this.changes.GetOrNull(interviewId) ?? new List<Action<InterviewReference>>();
            if (action != null)
            {
                changesList.Add(action);
            }
            changesList.Add(summary => summary.UpdateDateUtc = updateDate);
            this.changes[interviewId] = changesList;
        }

        public async Task SaveStateAsync(CancellationToken cancellationToken = default)
        {
            var db = this.dbContext;

            foreach (var change in changes)
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
