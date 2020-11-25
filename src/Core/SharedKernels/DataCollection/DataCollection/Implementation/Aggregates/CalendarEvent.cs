#nullable enable
using System;
using Main.Core.Events;
using Ncqrs.Domain;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent;
using WB.Core.SharedKernels.DataCollection.Events.CalendarEvent;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.CalendarEventInfrastructure;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public class CalendarEvent : EventSourcedAggregateRoot
    {
        internal readonly CalendarEventProperties properties = new CalendarEventProperties();

        public override Guid EventSourceId
        {
            get => base.EventSourceId;
            protected set
            {
                base.EventSourceId = value;
                this.properties.PublicKey = value;
            }
        }

        #region Apply

        protected void Apply(CalendarEventCreated @event)
        {
            this.properties.PublicKey = this.EventSourceId;
            this.properties.Start = @event.Start;
            this.properties.StartTimezone = @event.StartTimezone;
            this.properties.Comment = @event.Comment;
            this.properties.InterviewId = @event.InterviewId;
            this.properties.AssignmentId = @event.AssignmentId;
            this.properties.CreatedAt = @event.OriginDate;
            this.properties.UpdatedAt = @event.OriginDate;
            this.properties.InterviewKey = @event.InterviewKey;
        }
        
        protected void Apply(CalendarEventUpdated @event)
        {
            //ignore event if it occured before last change
            if (this.properties.UpdatedAt > @event.OriginDate) return;
            
            properties.Start = @event.Start;
            properties.StartTimezone = @event.StartTimezone;
            properties.Comment = @event.Comment;
            properties.UpdatedAt = @event.OriginDate;
        }
        
        protected void Apply(CalendarEventCompleted @event)
        {
            //ignore event if it occured before last change
            if (this.properties.UpdatedAt > @event.OriginDate) return;
            
            this.properties.IsCompleted = true;
            this.properties.UpdatedAt = @event.OriginDate;
        }
        
        protected void Apply(CalendarEventDeleted @event)
        {
            //ignore event if it occured before last change
            if (this.properties.UpdatedAt > @event.OriginDate) return;
            
            this.properties.IsDeleted = true;
            this.properties.UpdatedAt = @event.OriginDate;
        }  
        
        protected void Apply(CalendarEventRestored @event)
        {
            //ignore event if it occured before last change
            if (this.properties.UpdatedAt > @event.OriginDate) return;
            
            this.properties.IsDeleted = false;
            this.properties.UpdatedAt = @event.OriginDate;
        }
        
        #endregion

        public void CreateCalendarEvent(CreateCalendarEventCommand command)
        {
            ApplyEvent(new CalendarEventCreated(
                userId: command.UserId,
                originDate: command.OriginDate,
                comment: command.Comment,
                start: command.Start,
                startTimezone: command.StartTimezone,
                interviewId: command.InterviewId,
                interviewKey:command.InterviewKey,
                assignmentId: command.AssignmentId));
        }
        
        public void UpdateCalendarEvent(UpdateCalendarEventCommand command)
        {
            ApplyEvent(new CalendarEventUpdated(
                userId: command.UserId,
                originDate: command.OriginDate,
                comment: command.Comment,
                start: command.Start,
                startTimezone: command.StartTimezone));
        }

        public void CompleteCalendarEvent(CompleteCalendarEventCommand command)
        {
            ApplyEvent(new CalendarEventCompleted(
                userId: command.UserId,
                originDate: command.OriginDate));
        }
        
        public void DeleteCalendarEvent(DeleteCalendarEventCommand command)
        {
            ApplyEvent(new CalendarEventDeleted(
                userId: command.UserId,
                originDate: command.OriginDate));
        }

        public void SyncCalendarEventEvents(SyncCalendarEventEventsCommand command)
        {
            foreach (AggregateRootEvent synchronizedEvent in command.SynchronizedEvents)
            {
                var @event = synchronizedEvent.Payload;
                this.ApplyEvent(synchronizedEvent.EventIdentifier, synchronizedEvent.EventTimeStamp, @event);
            }
        }

        public void RestoreCalendarEvent(RestoreCalendarEventCommand command)
        {
            ApplyEvent(new CalendarEventRestored(
                userId: command.UserId,
                originDate: command.OriginDate));
        }
    }
}
