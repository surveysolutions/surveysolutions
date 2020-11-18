#nullable enable

using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Repositories
{
    public interface ICalendarEventStorage
    {
        CalendarEvent? GetCalendarEventForInterview(Guid interviewId);

        CalendarEvent? GetCalendarEventForAssigment(int assignmentId);
        void Store(CalendarEvent calendarEvent);
        CalendarEvent? GetById(Guid calendarEventId);
        void Remove(Guid calendarEventId);
        IEnumerable<CalendarEvent> GetNotSynchedCalendarEventsInOrder();
        IEnumerable<CalendarEvent> GetCalendarEventsForUser(Guid userId);

        IReadOnlyCollection<CalendarEvent> LoadAll();

        void SetCalendarEventSyncedStatus(Guid calendarEventId, bool isSynced);
    }
}
