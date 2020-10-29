using System;
using Ncqrs.Domain;
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
            this.properties.Comment = @event.Comment;

            properties.InterviewId = @event.InterviewId;
            properties.AssignmentId = @event.AssignmentId;
            this.properties.CreatedAt = @event.OriginDate;
            this.properties.UpdatedAt = @event.OriginDate;
        }
        
        protected void Apply(CalendarEventUpdated @event)
        {
            this.properties.Start = @event.Start;
            this.properties.Comment = @event.Comment;
            
            this.properties.UpdatedAt = @event.OriginDate;
        }
        
        protected void Apply(CalendarEventCompleted @event)
        {
            this.properties.IsCompleted = true;
            this.properties.UpdatedAt = @event.OriginDate;
        }
        
        protected void Apply(CalendarEventDeleted @event)
        {
            this.properties.IsDeleted = true;
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
                interviewId: command.InterviewId,
                assignmentId: command.AssignmentId));
        }
        
        public void UpdateCalendarEvent(UpdateCalendarEventCommand command)
        {
            ApplyEvent(new CalendarEventUpdated(
                userId: command.UserId,
                originDate: command.OriginDate,
                comment: command.Comment,
                start: command.Start));
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
    }
}
