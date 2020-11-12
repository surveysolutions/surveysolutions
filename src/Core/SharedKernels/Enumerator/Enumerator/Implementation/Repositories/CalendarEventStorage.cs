#nullable enable

using System;
using System.Collections.Generic;
using SQLite;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Repositories
{
    public class CalendarEventStorage : SqlitePlainStorage<CalendarEvent, Guid>, ICalendarEventStorage
    {
        public CalendarEventStorage(ILogger logger, IFileSystemAccessor fileSystemAccessor, SqliteSettings settings) 
            : base(logger, fileSystemAccessor, settings)
        {
        }

        public CalendarEventStorage(SQLiteConnectionWithLock storage, ILogger logger) : base(storage, logger)
        {
        }

        public CalendarEvent? GetCalendarEventForInterview(Guid interviewId)
        {
            return this.connection.Find<CalendarEvent>(e => 
                e.InterviewId == interviewId);
        }

        public CalendarEvent? GetCalendarEventForAssigment(long assignmentId)
        {
            return this.connection.Find<CalendarEvent>(e => 
                e.InterviewId == null && e.AssignmentId == assignmentId);
        }

        public IEnumerable<CalendarEvent> GetNotSynchedCalendarEvents()
        {
            return RunInTransaction(documents =>
            {
                var calendarEvents = documents.Connection.Table<CalendarEvent>()
                    .Where(a => a.IsSynchronized == false)
                    .ToList();

                return calendarEvents;
            });
        }        

        public IEnumerable<CalendarEvent> GetCalendarEventsForUser(Guid userId)
        {
            return RunInTransaction(documents =>
            {
                var calendarEvents = documents.Connection.Table<CalendarEvent>()
                    .Where(a => a.UserId == userId)
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

        public void RemoveCalendarEvent(Guid id)
        {
            this.aggregateRootRepositoryWithCache.CleanCache();

            this.interviewViewRepository.Remove(interviewId.FormatGuid());
            this.interviewSequenceViewRepository.Remove(interviewId);

            this.RemoveInterviewImages(interviewId);
            this.eventStorage.RemoveEventSourceById(interviewId);
            this.eventRegistry.RemoveAggregateRoot(interviewId.FormatGuid());
        }
    }
}
