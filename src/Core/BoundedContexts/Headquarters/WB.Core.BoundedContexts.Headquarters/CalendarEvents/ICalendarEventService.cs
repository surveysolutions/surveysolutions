#nullable enable
using System;

namespace WB.Core.BoundedContexts.Headquarters.CalendarEvents
{
    public interface ICalendarEventService
    {
        CalendarEvent? GetCalendarEventById(Guid id);
        CalendarEvent? GetActiveCalendarEventByInterviewId(Guid id);
    }
}
