#nullable enable
using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.CalendarEvents
{
    public interface ICalendarEventService
    {
        CalendarEvent? GetCalendarEventById(Guid id);
        CalendarEvent? GetActiveCalendarEventByInterviewId(Guid id);

        List<CalendarEvent> GetAllCalendarEventsForUser(Guid userId);
        List<CalendarEvent> GetAllCalendarEventsUnderSupervisor(Guid supervisorId);
    }
}
