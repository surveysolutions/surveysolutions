using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using SQLite;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.SynchronizationProcessTests
{
    [TestFixture]
    public class CalendarEventStorageTests
    {
        [Test]
        public void when_save_calendar_event_then_should_save_all_data_after_read()
        {
            var calendarEventStorage = CreateStorage();
            
            var calendarEvent = new CalendarEvent()
            {
                Id = Guid.NewGuid(),
                AssignmentId = 7,
                Comment = "comment",
                InterviewId = Guid.NewGuid(),
                InterviewKey = "key",
                IsCompleted = true,
                IsDeleted = true,
                IsSynchronized = true,
                LastEventId = Guid.NewGuid(),
                LastUpdateDateUtc = DateTime.UtcNow,
                Start = DateTimeOffset.UtcNow,
                StartTimezone = "zone",
                UserId = Guid.NewGuid(),
            };
            
            calendarEventStorage.Store(calendarEvent);

            var fromDb = calendarEventStorage.GetById(calendarEvent.Id);
            Assert.That(fromDb, Is.Not.Null);
            Assert.That(calendarEvent.Id, Is.EqualTo(fromDb.Id));
            Assert.That(calendarEvent.AssignmentId, Is.EqualTo(fromDb.AssignmentId));
            Assert.That(calendarEvent.Comment, Is.EqualTo(fromDb.Comment));
            Assert.That(calendarEvent.InterviewId, Is.EqualTo(fromDb.InterviewId));
            Assert.That(calendarEvent.InterviewKey, Is.EqualTo(fromDb.InterviewKey));
            Assert.That(calendarEvent.IsCompleted, Is.EqualTo(fromDb.IsCompleted));
            Assert.That(calendarEvent.IsDeleted, Is.EqualTo(fromDb.IsDeleted));
            Assert.That(calendarEvent.IsSynchronized, Is.EqualTo(fromDb.IsSynchronized));
            Assert.That(calendarEvent.LastEventId, Is.EqualTo(fromDb.LastEventId));
            Assert.That(calendarEvent.LastUpdateDateUtc, Is.EqualTo(fromDb.LastUpdateDateUtc));
            Assert.That(calendarEvent.Start, Is.EqualTo(fromDb.Start));
            Assert.That(calendarEvent.StartTimezone, Is.EqualTo(fromDb.StartTimezone));
            Assert.That(calendarEvent.UserId, Is.EqualTo(fromDb.UserId));
        }

        [Test]
        public void when_get_calendar_event_for_assignment_should_return_active_event_only()
        {
            var assignmentId = 5;
            var calendarEventStorage = CreateStorage();


            var deletedCalendarEvent = Create.Entity.CalendarEvent(assignmentId: assignmentId, isDeleted: true);
            var calendarEvent = Create.Entity.CalendarEvent(assignmentId: assignmentId);
            var completedCalendarEvent = Create.Entity.CalendarEvent(assignmentId: assignmentId, isCompleted: true);
            
            calendarEventStorage.Store(deletedCalendarEvent);
            calendarEventStorage.Store(calendarEvent);
            calendarEventStorage.Store(completedCalendarEvent);

            var fromDb = calendarEventStorage.GetCalendarEventForAssigment(assignmentId);

            Assert.That(fromDb, Is.Not.Null);
            Assert.That(calendarEvent.Id, Is.EqualTo(fromDb.Id));
            Assert.That(calendarEvent.AssignmentId, Is.EqualTo(fromDb.AssignmentId));
            Assert.That(calendarEvent.Comment, Is.EqualTo(fromDb.Comment));
            Assert.That(calendarEvent.InterviewId, Is.EqualTo(fromDb.InterviewId));
            Assert.That(calendarEvent.InterviewKey, Is.EqualTo(fromDb.InterviewKey));
            Assert.That(calendarEvent.IsCompleted, Is.EqualTo(fromDb.IsCompleted));
            Assert.That(calendarEvent.IsDeleted, Is.EqualTo(fromDb.IsDeleted));
            Assert.That(calendarEvent.IsSynchronized, Is.EqualTo(fromDb.IsSynchronized));
            Assert.That(calendarEvent.LastEventId, Is.EqualTo(fromDb.LastEventId));
            Assert.That(calendarEvent.LastUpdateDateUtc, Is.EqualTo(fromDb.LastUpdateDateUtc));
            Assert.That(calendarEvent.Start, Is.EqualTo(fromDb.Start));
            Assert.That(calendarEvent.StartTimezone, Is.EqualTo(fromDb.StartTimezone));
            Assert.That(calendarEvent.UserId, Is.EqualTo(fromDb.UserId));
        }

        [Test]
        public void when_get_calendar_event_for_interview_should_return_active_event_only()
        {
            var assignmentId = 5;
            var interviewId = Guid.NewGuid();
            var calendarEventStorage = CreateStorage();


            var deletedCalendarEvent = Create.Entity.CalendarEvent(assignmentId: assignmentId, interviewId: interviewId, isDeleted: true);
            var calendarEvent = Create.Entity.CalendarEvent(assignmentId: assignmentId, interviewId: interviewId);
            var completedCalendarEvent = Create.Entity.CalendarEvent(assignmentId: assignmentId, interviewId: interviewId, isCompleted: true);
            
            calendarEventStorage.Store(deletedCalendarEvent);
            calendarEventStorage.Store(calendarEvent);
            calendarEventStorage.Store(completedCalendarEvent);

            var fromDb = calendarEventStorage.GetCalendarEventForInterview(interviewId);

            Assert.That(fromDb, Is.Not.Null);
            Assert.That(calendarEvent.Id, Is.EqualTo(fromDb.Id));
            Assert.That(calendarEvent.AssignmentId, Is.EqualTo(fromDb.AssignmentId));
            Assert.That(calendarEvent.Comment, Is.EqualTo(fromDb.Comment));
            Assert.That(calendarEvent.InterviewId, Is.EqualTo(fromDb.InterviewId));
            Assert.That(calendarEvent.InterviewKey, Is.EqualTo(fromDb.InterviewKey));
            Assert.That(calendarEvent.IsCompleted, Is.EqualTo(fromDb.IsCompleted));
            Assert.That(calendarEvent.IsDeleted, Is.EqualTo(fromDb.IsDeleted));
            Assert.That(calendarEvent.IsSynchronized, Is.EqualTo(fromDb.IsSynchronized));
            Assert.That(calendarEvent.LastEventId, Is.EqualTo(fromDb.LastEventId));
            Assert.That(calendarEvent.LastUpdateDateUtc, Is.EqualTo(fromDb.LastUpdateDateUtc));
            Assert.That(calendarEvent.Start, Is.EqualTo(fromDb.Start));
            Assert.That(calendarEvent.StartTimezone, Is.EqualTo(fromDb.StartTimezone));
            Assert.That(calendarEvent.UserId, Is.EqualTo(fromDb.UserId));
        }

        [Test]
        public void when_get_not_synch_calendar_events_should_return_all_events_with_unchich_flag_in_lastupdate_order()
        {
            var calendarEventStorage = CreateStorage();

            var utcNow = DateTime.UtcNow;
            var deletedCalendarEvent = Create.Entity.CalendarEvent(isSynchronized:false, lastUpdate: utcNow.AddDays(-8), isDeleted: true);
            var unsyncCalendarEvent = Create.Entity.CalendarEvent(isSynchronized:false, lastUpdate: utcNow.AddDays(-3));
            var syncCalendarEvent = Create.Entity.CalendarEvent(isSynchronized:true, lastUpdate: utcNow.AddDays(-5));
            var completedCalendarEvent = Create.Entity.CalendarEvent(isSynchronized: false, lastUpdate: utcNow.AddDays(-10), isCompleted: true);
            
            calendarEventStorage.Store(deletedCalendarEvent);
            calendarEventStorage.Store(unsyncCalendarEvent);
            calendarEventStorage.Store(syncCalendarEvent);
            calendarEventStorage.Store(completedCalendarEvent);

            var fromDb = calendarEventStorage.GetNotSynchedCalendarEventsInOrder().ToList();

            Assert.That(fromDb, Is.Not.Null);
            Assert.That(fromDb.Count, Is.EqualTo(3));
            Assert.That(fromDb[0].Id, Is.EqualTo(completedCalendarEvent.Id));
            Assert.That(fromDb[1].Id, Is.EqualTo(deletedCalendarEvent.Id));
            Assert.That(fromDb[2].Id, Is.EqualTo(unsyncCalendarEvent.Id));
        }

        private ICalendarEventStorage CreateStorage()
        {
            var connection = Create.Storage.InMemorySqLiteConnection;
            var storage = new CalendarEventStorage(connection, Mock.Of<ILogger>());
            return storage;
        }
    }
}