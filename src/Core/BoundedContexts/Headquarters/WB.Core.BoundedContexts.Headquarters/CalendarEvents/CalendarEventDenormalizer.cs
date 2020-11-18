using System;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.CalendarEvent;

namespace WB.Core.BoundedContexts.Headquarters.CalendarEvents
{
    public class CalendarEventDenormalizer : AbstractFunctionalEventHandlerOnGuid<CalendarEvent, IReadSideRepositoryWriter<CalendarEvent, Guid>>,
        IUpdateHandler<CalendarEvent, CalendarEventCreated>,
        IUpdateHandler<CalendarEvent, CalendarEventDeleted>,
        IUpdateHandler<CalendarEvent, CalendarEventUpdated>,
        IUpdateHandler<CalendarEvent, CalendarEventCompleted>
    {
        public CalendarEventDenormalizer(IReadSideRepositoryWriter<CalendarEvent, Guid> readSideStorage) : base(readSideStorage)
        {
        }

        public CalendarEvent Update(CalendarEvent state, IPublishedEvent<CalendarEventCreated> @event)
        {
            state = new CalendarEvent(
                @event.EventSourceId,
                @event.Payload.Start.UtcDateTime,
                @event.Payload.Comment,
                @event.Payload.InterviewId,
                @event.Payload.AssignmentId,
                false,
                @event.Payload.OriginDate,
                @event.Payload.UserId,
                "");

            return state;

        }

        public CalendarEvent Update(CalendarEvent state, IPublishedEvent<CalendarEventDeleted> @event)
        {
            return UpdateCalendarEvent(state, @event.Payload.OriginDate,
                calendarEvent =>
                {
                    calendarEvent.IsDeleted = true;
                });
        }

        public CalendarEvent Update(CalendarEvent state, IPublishedEvent<CalendarEventUpdated> @event)
        {
            return UpdateCalendarEvent(state, @event.Payload.OriginDate,
                calendarEvent =>
                {
                    calendarEvent.Start = @event.Payload.Start.UtcDateTime;
                    calendarEvent.Comment = @event.Payload.Comment;
                });
        }

        public CalendarEvent Update(CalendarEvent state, IPublishedEvent<CalendarEventCompleted> @event)
        {
            return UpdateCalendarEvent(state, @event.Payload.OriginDate,
                calendarEvent =>
                {
                    calendarEvent.IsCompleted = true;
                });
        }
        private CalendarEvent UpdateCalendarEvent(CalendarEvent calendarEvent, DateTimeOffset dateTimeOffset, Action<CalendarEvent> updater)
        {
            if (calendarEvent.UpdateDate > dateTimeOffset.UtcDateTime) return calendarEvent;
            
            updater.Invoke(calendarEvent);
            calendarEvent.UpdateDate = dateTimeOffset.UtcDateTime;

            return calendarEvent;
        }
    }
}
