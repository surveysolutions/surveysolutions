#nullable enable

using System;
using System.Collections.Generic;
using SQLite;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Repositories
{
    public class CalendarEventStorage : SqlitePlainStorage<CalendarEvent, Guid>, ICalendarEventStorage
    {
        public CalendarEventStorage(ILogger logger, IFileSystemAccessor fileSystemAccessor, SqliteSettings settings) : base(logger, fileSystemAccessor, settings)
        {
        }

        public CalendarEventStorage(SQLiteConnectionWithLock storage, ILogger logger) : base(storage, logger)
        {
        }

        public CalendarEvent? GetEventForInterview(Guid interviewId)
        {
            return this.connection.Find<CalendarEvent>(e => 
                e.InterviewId == interviewId);
        }

        public CalendarEvent? GetEventForAssigment(long assignmentId)
        {
            return this.connection.Find<CalendarEvent>(e => 
                e.InterviewId == null && e.AssignmentId == assignmentId);
        }

        public IEnumerable<CalendarEvent> GetNotSynchedCalendarEvents(Guid userId)
        {
            throw new NotImplementedException();
        }
    }
}