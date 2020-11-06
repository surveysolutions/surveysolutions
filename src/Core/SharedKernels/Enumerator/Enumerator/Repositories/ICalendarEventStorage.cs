#nullable enable

using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Repositories
{
    public interface ICalendarEventStorage
    {
        CalendarEvent? GetEventForInterview(Guid interviewId);

        CalendarEvent? GetEventForAssigment(long assignmentId);
        void Store(CalendarEvent calendarEvent);
        CalendarEvent GetById(Guid calendarEventId);
        void Remove(Guid calendarEventId);
        IEnumerable<CalendarEvent> GetNotSynchedCalendarEvents(Guid userId);
    }
}