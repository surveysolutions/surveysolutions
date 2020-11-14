using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection.Events.CalendarEvent;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Denormalizer
{
    public class CalendarEventEventHandler : BaseDenormalizer, 
                                         IEventHandler<CalendarEventCreated>,
                                         IEventHandler<CalendarEventUpdated>,
                                         IEventHandler<CalendarEventCompleted>,
                                         IEventHandler<CalendarEventDeleted>
    {
        private readonly ICalendarEventStorage calendarEventStorage;

        public CalendarEventEventHandler(ICalendarEventStorage calendarEventStorage)
        {
            this.calendarEventStorage = calendarEventStorage;
        }

        public void Handle(IPublishedEvent<CalendarEventCreated> evnt)
        {
            CalendarEvent calendarEvent = new CalendarEvent()
            {
                Id = evnt.EventSourceId,
                AssignmentId = evnt.Payload.AssignmentId,
                InterviewId = evnt.Payload.InterviewId,
                Comment = evnt.Payload.Comment,
                Start = evnt.Payload.Start,
                IsCompleted = false,
                IsSynchronized = false,
                LastUpdateDate = evnt.EventTimeStamp,
                UserId = evnt.Payload.UserId,
                LastEventId = evnt.EventIdentifier,
            };
            calendarEventStorage.Store(calendarEvent);
        }

        public void Handle(IPublishedEvent<CalendarEventUpdated> evnt)
        {
            CalendarEvent calendarEvent = calendarEventStorage.GetById(evnt.EventSourceId);
            calendarEvent.Start = evnt.Payload.Start;
            calendarEvent.Comment = evnt.Payload.Comment;
            calendarEvent.UserId = evnt.Payload.UserId;
            calendarEvent.LastEventId = evnt.EventIdentifier;
            calendarEvent.IsSynchronized = false;
            calendarEventStorage.Store(calendarEvent);
        }

        public void Handle(IPublishedEvent<CalendarEventCompleted> evnt)
        {
            CalendarEvent calendarEvent = calendarEventStorage.GetById(evnt.EventSourceId);
            calendarEvent.IsCompleted = true;
            calendarEvent.LastEventId = evnt.EventIdentifier;
            calendarEvent.IsSynchronized = false;
            calendarEventStorage.Store(calendarEvent);
        }

        public void Handle(IPublishedEvent<CalendarEventDeleted> evnt)
        {
            CalendarEvent calendarEvent = calendarEventStorage.GetById(evnt.EventSourceId);
            calendarEvent.IsDeleted = true;
            calendarEvent.LastEventId = evnt.EventIdentifier;
            calendarEvent.IsSynchronized = false;
            calendarEventStorage.Store(calendarEvent);
        }
    }
}
