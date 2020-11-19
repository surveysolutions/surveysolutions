#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.CalendarEvents
{
    class CalendarEventService : ICalendarEventService
    {
        private readonly IQueryableReadSideRepositoryReader<CalendarEvent, Guid> calendarEventsAccessor;
        private readonly IUserRepository userRepository;
        private readonly IQueryableReadSideRepositoryReader<Assignment, Guid> assignmentsAccessor;
        private readonly InterviewerInterviewsFactory interviewerInterviewsFactory;
        

        public CalendarEventService(IQueryableReadSideRepositoryReader<CalendarEvent, Guid> calendarEventsAccessor,
            IUserRepository userRepository, IQueryableReadSideRepositoryReader<Assignment, Guid> assignmentsAccessor, InterviewerInterviewsFactory interviewerInterviewsFactory)
        {
            this.calendarEventsAccessor = calendarEventsAccessor ?? throw new ArgumentNullException(nameof(calendarEventsAccessor));
            this.userRepository = userRepository;
            this.assignmentsAccessor = assignmentsAccessor;
            this.interviewerInterviewsFactory = interviewerInterviewsFactory;
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

        public List<CalendarEvent> GetAllCalendarEventsForUser(Guid responsibleId)
        {
            var userAssignmentsIds = this.assignmentsAccessor.Query(x =>
                x.Where(assignment =>
                        assignment.ResponsibleId == responsibleId
                        && !assignment.Archived
                        && (assignment.Quantity == null || assignment.InterviewSummaries.Count < assignment.Quantity)
                        && (assignment.WebMode == null || assignment.WebMode == false))
                    .Select(x => x.Id)
                    .ToList());
            
            var calendarEventsByAssignments = calendarEventsAccessor.Query(_ => 
                _.Where(x =>
                    x.IsDeleted != true 
                    && x.IsCompleted != true
                    && x.InterviewId == null
                    && userAssignmentsIds.Contains(x.AssignmentId) )
                    .Select(x => x).ToList());

            var userInterviewIds 
                = interviewerInterviewsFactory.GetInProgressInterviewsForInterviewer(responsibleId)
                    .Select(x=>x.Id as Guid?).ToList();
            
            var calendarEventsByInterviews = calendarEventsAccessor.Query(_ => 
                _.Where(x =>
                        x.IsDeleted != true 
                        && x.IsCompleted != true
                        && x.InterviewId != null
                        && userInterviewIds.Contains(x.InterviewId) )
                    .Select(x => x).ToList());
            
            return calendarEventsByInterviews.Union(calendarEventsByAssignments).ToList();
        }

        public List<CalendarEvent> GetAllCalendarEventsUnderSupervisor(Guid supervisorId)
        {
            var assignmentdForSupervisorIds =this.assignmentsAccessor.Query(x =>
                x.Where(assignment =>
                        (assignment.ResponsibleId == supervisorId || assignment.Responsible.ReadonlyProfile.SupervisorId == supervisorId)
                        && !assignment.Archived
                        && (assignment.Quantity == null || assignment.InterviewSummaries.Count < assignment.Quantity)
                        && (assignment.WebMode == null || assignment.WebMode == false))
                    .Select(x=>x.Id)
                    .ToList());
            
            var calendarEventsByAssignments = calendarEventsAccessor.Query(_ => 
                _.Where(x =>
                        x.IsDeleted != true 
                        && x.IsCompleted != true
                        && x.InterviewId == null
                        && assignmentdForSupervisorIds.Contains(x.AssignmentId) )
                    .Select(x => x).ToList());

            var teamInterviewIds =
                interviewerInterviewsFactory.GetInProgressInterviewsForSupervisor(supervisorId)
                    .Select(x=>x.Id as Guid?).ToList();
            
            var calendarEventsByInterviews = calendarEventsAccessor.Query(_ => 
                _.Where(x =>
                        x.IsDeleted != true 
                        && x.IsCompleted != true
                        && x.InterviewId != null
                        && teamInterviewIds.Contains(x.InterviewId) )
                    .Select(x => x).ToList());

            return calendarEventsByInterviews.Union(calendarEventsByAssignments).ToList();
        }
    }
}
