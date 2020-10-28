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
        }
        
        protected void Apply(CalendarEventUpdated @event)
        {
            this.properties.Start = @event.Start;
            this.properties.Comment = @event.Comment;
        }
        #endregion

        public void CreateCalendarEvent(CreateCalendarEventCommand command)
        {
            ApplyEvent(new CalendarEventCreated(
                userId: command.UserId,
                originDate: command.OriginDate,
                comment: command.Comment,
                start: command.Start));
        }
        
        public void UpdateCalendarEvent(UpdateCalendarEventCommand command)
        {
            ApplyEvent(new CalendarEventUpdated(
                userId: command.UserId,
                originDate: command.OriginDate,
                comment: command.Comment,
                start: command.Start));
        }
    }
}
