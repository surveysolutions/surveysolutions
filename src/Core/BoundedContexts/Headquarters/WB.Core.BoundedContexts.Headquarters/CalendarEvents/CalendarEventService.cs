#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.CalendarEvents
{
    class CalendarEventService : ICalendarEventService
    {
        private readonly IQueryableReadSideRepositoryReader<CalendarEvent, Guid> calendarEventsAccessor;
        private readonly IQueryableReadSideRepositoryReader<Assignment, Guid> assignmentsAccessor;
        private readonly IInterviewInformationFactory interviewerInterviewsFactory;
        

        public CalendarEventService(IQueryableReadSideRepositoryReader<CalendarEvent, Guid> calendarEventsAccessor,
            IQueryableReadSideRepositoryReader<Assignment, Guid> assignmentsAccessor, 
            IInterviewInformationFactory interviewerInterviewsFactory)
        {
            this.calendarEventsAccessor = 
                calendarEventsAccessor ?? throw new ArgumentNullException(nameof(calendarEventsAccessor));
            this.assignmentsAccessor = 
                assignmentsAccessor ?? throw new ArgumentNullException(nameof(assignmentsAccessor));
            this.interviewerInterviewsFactory = 
                interviewerInterviewsFactory ?? throw new ArgumentNullException(nameof(interviewerInterviewsFactory));
        }

        public CalendarEvent? GetCalendarEventById(Guid id)
        {
            return this.calendarEventsAccessor.GetById(id);
        }

        public CalendarEvent? GetActiveCalendarEventForInterviewId(Guid id)
        {
            return this.calendarEventsAccessor.Query<CalendarEvent?>(
                x => x.FirstOrDefault(y => 
                    y.InterviewId == id
                    && y.DeletedAtUtc == null
                    && y.CompletedAtUtc == null));
        }
        
        public CalendarEvent? GetActiveCalendarEventForAssignmentId(int id)
        {
            return this.calendarEventsAccessor.Query<CalendarEvent?>(
                x => x.FirstOrDefault(y => 
                    y.InterviewId == null 
                    && y.AssignmentId == id
                    && y.DeletedAtUtc == null
                    && y.CompletedAtUtc == null));
        }

        public List<CalendarEvent> GetAllCalendarEventsForInterviewer(Guid interviewerId)
        {
            return GetAllCalendarEventsFor(interviewerId: interviewerId);
        }

        public List<CalendarEvent> GetAllCalendarEventsUnderSupervisor(Guid supervisorId)
        {
            return GetAllCalendarEventsFor(supervisorId: supervisorId);
        }
        
        private List<CalendarEvent> GetAllCalendarEventsFor(Guid? interviewerId = null, Guid? supervisorId = null)
        {
            if (interviewerId.HasValue && supervisorId.HasValue)
                throw new ArgumentException("Only one parameter required");
            if (!interviewerId.HasValue && !supervisorId.HasValue)
                throw new ArgumentException("Required one parameter");
        
            var userAssignmentsIds = this.assignmentsAccessor.Query(x =>
            {
                var filter = x;

                if (interviewerId.HasValue)
                    filter = filter.Where(assignment => assignment.ResponsibleId == interviewerId);
                else if (supervisorId.HasValue)
                    filter = filter.Where(assignment => assignment.ResponsibleId == supervisorId || assignment.Responsible.ReadonlyProfile.SupervisorId == supervisorId);
                
                return filter.Where(assignment =>
                        !assignment.Archived
                        && (assignment.Quantity == null || assignment.InterviewSummaries.Count < assignment.Quantity)
                        && (assignment.WebMode == null || assignment.WebMode == false))
                    .Select(x => x.Id)
                    .ToList();
            });
            
            var calendarEventsByAssignments = calendarEventsAccessor.Query(_ => 
                _.Where(x =>
                    x.DeletedAtUtc == null 
                    && x.CompletedAtUtc == null
                    && x.InterviewId == null
                    && userAssignmentsIds.Contains(x.AssignmentId) )
                    .Select(x => x).ToList());

            var inProgressInterviews = interviewerId.HasValue
                ? interviewerInterviewsFactory.GetInProgressInterviewsForInterviewer(interviewerId.Value)
                : interviewerInterviewsFactory.GetInProgressInterviewsForSupervisor(supervisorId!.Value);
            var userInterviewIds = inProgressInterviews.Select(x=>x.Id as Guid?).ToList();
            
            var calendarEventsByInterviews = calendarEventsAccessor.Query(_ => 
                _.Where(x =>
                        x.DeletedAtUtc == null 
                        && x.CompletedAtUtc == null
                        && x.InterviewId != null
                        && userInterviewIds.Contains(x.InterviewId) )
                    .Select(x => x).ToList());
            
            var calendarEventsWithoutInterviews = calendarEventsAccessor
                .Query(_ =>
                {
                    var filter = _;

                    if (interviewerId.HasValue)
                        filter = filter.Where(x => x.CreatorUserId == interviewerId);
                    else if (supervisorId.HasValue)
                        filter = filter.Where(x => x.CreatorUserId == supervisorId || x.Creator!.ReadonlyProfile.SupervisorId == supervisorId);

                    return filter.Where(x =>
                            x.DeletedAtUtc == null
                            && x.CompletedAtUtc == null
                            && x.InterviewId != null
                            && !userInterviewIds.Contains(x.InterviewId))
                        .Select(x => x).ToList();
                });

            var checkReassignInterviews = interviewerInterviewsFactory.GetInterviewsByIds(
                calendarEventsWithoutInterviews.Select(c => c.InterviewId!.Value).ToArray());
            var skipIds = checkReassignInterviews.Where(i => i != null)
                .Select(i => i.Id).ToList();
            var filteredCalendarEventsWithoutInterviews = calendarEventsWithoutInterviews
                .Where(c => !skipIds.Contains(c.InterviewId!.Value)).ToList();

            return calendarEventsByInterviews
                .Union(calendarEventsByAssignments)
                .Union(filteredCalendarEventsWithoutInterviews)
                .ToList();
        }

    }
}
