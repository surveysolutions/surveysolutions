#nullable enable

using System;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Repositories
{
    public interface ICalendarEventStorage
    {
        CalendarEvent? GetEventForInterview(Guid interviewId);

        CalendarEvent? GetEventForAssigment(long assignmentId);
    }
}