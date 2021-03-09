#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Repositories
{
    public class CalendarEventStorage : SqlitePlainStorage<CalendarEvent, Guid>, ICalendarEventStorage
    {
        public CalendarEventStorage(ILogger logger, IFileSystemAccessor fileSystemAccessor, SqliteSettings settings,
            IWorkspaceAccessor workspaceAccessor) 
            : base(logger, fileSystemAccessor, settings, workspaceAccessor)
        {
        }

        public CalendarEventStorage(SQLiteConnectionWithLock storage, ILogger logger) : base(storage, logger)
        {
        }

        public CalendarEvent? GetCalendarEventForInterview(Guid interviewId)
        {
            return this.GetConnection().Table<CalendarEvent>()
                .SingleOrDefault(e =>  
                    e.InterviewId == interviewId
                    && e.IsDeleted == false
                    && e.IsCompleted == false);
        }

        public CalendarEvent? GetCalendarEventForAssigment(int assignmentId)
        {
            return this.GetConnection().Table<CalendarEvent>()
                .SingleOrDefault(e => 
                    e.InterviewId == null 
                    && e.AssignmentId == assignmentId
                    && e.IsDeleted == false
                    && e.IsCompleted == false);
        }

        public IEnumerable<CalendarEvent> GetNotSynchedCalendarEventsInOrder()
        {
            return RunInTransaction(documents =>
            {
                var calendarEvents = documents.Connection.Table<CalendarEvent>()
                    .Where(a => a.IsSynchronized == false)
                    .OrderBy(e => e.LastUpdateDateUtc)
                    .ToList();

                return calendarEvents;
            });
        }        

        public IEnumerable<CalendarEvent> GetCalendarEventsForUser(Guid userId)
        {
            return RunInTransaction(documents =>
            {
                var calendarEvents = documents.Connection.Table<CalendarEvent>()
                    .Where(calendarEvent => calendarEvent.UserId == userId
                                        && calendarEvent.IsDeleted != true
                                        && calendarEvent.IsCompleted != true)
                    .OrderBy(calendarEvent => calendarEvent.LastUpdateDateUtc)
                    .ToList();

                return calendarEvents;
            });
        }

        public void SetCalendarEventSyncedStatus(Guid calendarEventId, bool isSynced)
        {
            CalendarEvent calendarEvent = this.GetById(calendarEventId);
            if (calendarEvent == null) return;
            calendarEvent.IsSynchronized = isSynced;
            this.Store(calendarEvent);
        }
    }
}
