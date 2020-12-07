#nullable enable
using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.CalendarEvents
{
    public interface ICalendarEventService
    {
        CalendarEvent? GetCalendarEventById(Guid id);
        CalendarEvent? GetActiveCalendarEventForInterviewId(Guid id);
        CalendarEvent? GetActiveCalendarEventForAssignmentId(int id);
        
        List<CalendarEvent> GetAllCalendarEventsForInterviewer(Guid interviewerId);
        List<CalendarEvent> GetAllCalendarEventsUnderSupervisor(Guid supervisorId);
    }
}
