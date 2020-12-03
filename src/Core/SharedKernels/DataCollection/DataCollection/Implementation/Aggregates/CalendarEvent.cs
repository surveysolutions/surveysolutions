#nullable enable
using System;
using Main.Core.Events;
using Ncqrs.Domain;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent;
using WB.Core.SharedKernels.DataCollection.Events.CalendarEvent;
using WB.Core.SharedKernels.DataCollection.Exceptions;
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
            //if (this.properties.UpdatedAt > @event.OriginDate) return;
            
            properties.Start = @event.Start;
            properties.StartTimezone = @event.StartTimezone;
            properties.Comment = @event.Comment;
            properties.UpdatedAt = @event.OriginDate;
        }
        
        protected void Apply(CalendarEventCompleted @event)
        {
            //ignore event if it occured before last change
            //if (this.properties.UpdatedAt > @event.OriginDate) return;
            
            this.properties.IsCompleted = true;
            this.properties.UpdatedAt = @event.OriginDate;
        }
        
        protected void Apply(CalendarEventDeleted @event)
        {
            //ignore event if it occured before last change
            //if (this.properties.UpdatedAt > @event.OriginDate) return;
            
            this.properties.IsDeleted = true;
            this.properties.UpdatedAt = @event.OriginDate;
        }  
        
        protected void Apply(CalendarEventRestored @event)
        {
            //ignore event if it occured before last change
            //if (this.properties.UpdatedAt > @event.OriginDate) return;
            
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
            ThrowIsDeleted();
            
            ApplyEvent(new CalendarEventUpdated(
                userId: command.UserId,
                originDate: command.OriginDate,
                comment: command.Comment,
                start: command.Start,
                startTimezone: command.StartTimezone,
                false));
        }


        public void CompleteCalendarEvent(CompleteCalendarEventCommand command)
        {
            ThrowIsDeleted();
            
            ApplyEvent(new CalendarEventCompleted(
                userId: command.UserId,
                originDate: command.OriginDate));
        }
        
        public void DeleteCalendarEvent(DeleteCalendarEventCommand command)
        {
            ThrowIsDeleted();
            
            ApplyEvent(new CalendarEventDeleted(
                userId: command.UserId,
                originDate: command.OriginDate));
        }

        public void SyncCalendarEventEvents(SyncCalendarEventEventsCommand command)
        {
            var propertiesSnapshot = properties.Clone();
            
            if (command.RestoreCalendarEventBefore)
                ApplyEvent(new CalendarEventRestored(command.UserId, command.OriginDate));
            
            foreach (AggregateRootEvent synchronizedEvent in command.SynchronizedEvents)
            {
                var @event = synchronizedEvent.Payload;
                this.ApplyEvent(synchronizedEvent.EventIdentifier, synchronizedEvent.EventTimeStamp, @event);
            }

            if (command.DeleteCalendarEventAfter)
                ApplyEvent(new CalendarEventDeleted(command.UserId, command.OriginDate));
            if (command.RestoreCalendarEventAfter)
                ApplyEvent(new CalendarEventRestored(command.UserId, command.OriginDate));
            
            if (command.ShouldRestorePreviousStateAfterApplying && !command.DeleteCalendarEventAfter)
                ApplyEvent(new CalendarEventUpdated(command.UserId, DateTimeOffset.Now, 
                    propertiesSnapshot.Comment, propertiesSnapshot.Start, propertiesSnapshot.StartTimezone, true));
        }

        public void RestoreCalendarEvent(RestoreCalendarEventCommand command)
        {
            ThrowIsNotDeleted();
            
            ApplyEvent(new CalendarEventRestored(
                userId: command.UserId,
                originDate: command.OriginDate));
        }

        private void ThrowIsDeleted()
        {
            if (properties.IsDeleted)
                throw new CalendarEventException("Calendar event is deleted",
                    CalendarEventDomainExceptionType.CalendarEventIsDeleted);
        }

        private void ThrowIsNotDeleted()
        {
            if (!properties.IsDeleted)
                throw new CalendarEventException("Calendar event must be deleted",
                    CalendarEventDomainExceptionType.Undefined);
        }
    }
}
