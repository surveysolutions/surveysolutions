#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.CalendarEvents
{
    class CalendarEventService : ICalendarEventService
    {
        private readonly IQueryableReadSideRepositoryReader<CalendarEvent> calendarEventsAccessor;
        private readonly IUserRepository userRepository;

        public CalendarEventService(IQueryableReadSideRepositoryReader<CalendarEvent> calendarEventsAccessor,
            IUserRepository userRepository)
        {
            this.calendarEventsAccessor = calendarEventsAccessor ?? throw new ArgumentNullException(nameof(calendarEventsAccessor));
            this.userRepository = userRepository;
        }

        public CalendarEvent? GetCalendarEventById(Guid id)
        {
            return this.calendarEventsAccessor.GetById(id);
        }

        public CalendarEvent? GetActiveCalendarEventForInterviewId(Guid id)
        {
            return this.calendarEventsAccessor.Query<CalendarEvent>(
                x => x.FirstOrDefault(y => y.InterviewId == id));
        }
        
        public CalendarEvent? GetActiveCalendarEventForAssignmentId(int id)
        {
            return this.calendarEventsAccessor.Query<CalendarEvent>(
                x => x.FirstOrDefault(y => y.InterviewId == null && y.AssignmentId == id));
        }

        public List<CalendarEvent> GetAllCalendarEventsForUser(Guid userId)
        {
            var calendarEvents = calendarEventsAccessor.Query(_ => 
                _.Where(x =>
                    x.IsDeleted != true 
                    && x.IsCompleted != true
                    && x.UserId == userId)
                    .Select(x => x).ToList());

            return calendarEvents;
        }

        public List<CalendarEvent> GetAllCalendarEventsUnderSupervisor(Guid supervisorId)
        {
            var interviewers = userRepository.Users
                .Where(u => u.Profile.SupervisorId == supervisorId)
                .Select(u => u.Id);

            var calendarEvents = calendarEventsAccessor.Query(_ => _
                .Where(x =>
                        x.IsDeleted != true 
                        && x.IsCompleted != true
                        && interviewers.Contains(x.UserId))
                .Select(x => x)
                .ToList());

            return calendarEvents;
        }
    }
}
