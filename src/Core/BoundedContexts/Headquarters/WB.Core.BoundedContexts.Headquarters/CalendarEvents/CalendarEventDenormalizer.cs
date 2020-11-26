#nullable enable
using System;
using Ncqrs.Eventing.ServiceModel.Bus;
using NodaTime;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.CalendarEvent;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Core.BoundedContexts.Headquarters.CalendarEvents
{
    public class CalendarEventDenormalizer : AbstractFunctionalEventHandlerOnGuid<CalendarEvent, IReadSideRepositoryWriter<CalendarEvent, Guid>>,
        IUpdateHandler<CalendarEvent, CalendarEventCreated>,
        IUpdateHandler<CalendarEvent, CalendarEventDeleted>,
        IUpdateHandler<CalendarEvent, CalendarEventUpdated>,
        IUpdateHandler<CalendarEvent, CalendarEventCompleted>,
        IUpdateHandler<CalendarEvent, CalendarEventRestored>
    {
        public CalendarEventDenormalizer(IReadSideRepositoryWriter<CalendarEvent, Guid> readSideStorage) : base(readSideStorage)
        {
        }

        public CalendarEvent Update(CalendarEvent state, IPublishedEvent<CalendarEventCreated> @event)
        {
            state = new CalendarEvent(
                @event.EventSourceId,
                new ZonedDateTime(Instant.FromDateTimeOffset(@event.Payload.Start), DateTimeZoneProviders.Tzdb[@event.Payload.StartTimezone]),
                @event.Payload.Comment,
                @event.Payload.InterviewId,
                @event.Payload.InterviewKey,
                @event.Payload.AssignmentId,
                @event.Payload.OriginDate,
                @event.Payload.UserId);

            return state;
        }

        public CalendarEvent Update(CalendarEvent state, IPublishedEvent<CalendarEventDeleted> @event)
        {
            return UpdateCalendarEvent(state, @event.Payload.OriginDate,
                calendarEvent =>
                {
                    calendarEvent.DeletedAtUtc = @event.Payload.OriginDate.UtcDateTime;
                });
        }
        
        public CalendarEvent Update(CalendarEvent state, IPublishedEvent<CalendarEventRestored> @event)
        {
            return UpdateCalendarEvent(state, @event.Payload.OriginDate,
                calendarEvent =>
                {
                    calendarEvent.DeletedAtUtc = null;
                });
        }

        public CalendarEvent Update(CalendarEvent state, IPublishedEvent<CalendarEventUpdated> @event)
        {
            return UpdateCalendarEvent(state, @event.Payload.OriginDate,
                calendarEvent =>
                {
                    calendarEvent.Start = new ZonedDateTime(Instant.FromDateTimeOffset(@event.Payload.Start),
                        DateTimeZoneProviders.Tzdb[@event.Payload.StartTimezone]);
                    calendarEvent.Comment = @event.Payload.Comment;
                });
        }

        public CalendarEvent Update(CalendarEvent state, IPublishedEvent<CalendarEventCompleted> @event)
        {
            return UpdateCalendarEvent(state, @event.Payload.OriginDate,
                calendarEvent =>
                {
                    calendarEvent.CompletedAtUtc = @event.Payload.OriginDate.UtcDateTime;
                });
        }
        private CalendarEvent UpdateCalendarEvent(CalendarEvent calendarEvent, DateTimeOffset dateTimeOffset, Action<CalendarEvent> updater)
        {
            if (calendarEvent.UpdateDateUtc > dateTimeOffset.UtcDateTime) return calendarEvent;
            
            updater.Invoke(calendarEvent);
            calendarEvent.UpdateDateUtc = dateTimeOffset.UtcDateTime;

            return calendarEvent;
        }
    }
}
