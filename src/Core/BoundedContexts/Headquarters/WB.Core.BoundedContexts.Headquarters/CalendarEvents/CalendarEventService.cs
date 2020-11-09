#nullable enable
using System;
using System.Linq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.CalendarEvents
{
    class CalendarEventService : ICalendarEventService
    {
        private readonly IQueryableReadSideRepositoryReader<CalendarEvent> calendarEventsAccessor;

        public CalendarEventService(IQueryableReadSideRepositoryReader<CalendarEvent> calendarEventsAccessor)
        {
            this.calendarEventsAccessor = calendarEventsAccessor ?? throw new ArgumentNullException(nameof(calendarEventsAccessor));
        }

        public CalendarEvent? GetCalendarEventById(Guid id)
        {
            return this.calendarEventsAccessor.GetById(id);
        }

        public CalendarEvent? GetActiveCalendarEventByInterviewId(Guid id)
        {
            return this.calendarEventsAccessor.Query<CalendarEvent>(
                x => x.FirstOrDefault(y => y.InterviewId == id));
        }
    }
}
