using System;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Repositories
{
    public class CalendarEventRemoval : ICalendarEventRemoval
    {
        private readonly ICalendarEventStorage calendarEventStorage;
        private readonly IEnumeratorEventStorage eventStorage;
        private readonly IAssignmentDocumentsStorage assignmentDocumentsStorage;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly ILogger logger;

        public CalendarEventRemoval(ICalendarEventStorage calendarEventStorage,
            IEnumeratorEventStorage eventStorage,
            IAssignmentDocumentsStorage assignmentDocumentsStorage,
            IPlainStorage<InterviewView> interviewViewRepository,
            ILogger logger)
        {
            this.calendarEventStorage = calendarEventStorage;
            this.eventStorage = eventStorage;
            this.assignmentDocumentsStorage = assignmentDocumentsStorage;
            this.interviewViewRepository = interviewViewRepository;
            this.logger = logger;
        }

        public void Remove(Guid id)
        {
            var calendarEvent = calendarEventStorage.GetById(id);
            
            logger.Debug($"Removing Calendar Event {id}");

            if (calendarEvent != null)
            {
                if (calendarEvent.InterviewId.HasValue)
                {
                    var interviewView = interviewViewRepository.GetById(calendarEvent.InterviewId.Value.FormatGuid());
                    interviewView.CalendarEvent = null;
                    interviewView.CalendarEventId = null;
                    interviewView.CalendarEventTimezoneId = null;
                    interviewView.CalendarEventComment = null;
                    interviewView.CalendarEventLastUpdate = null;
                    interviewViewRepository.Store(interviewView);
                }
                else 
                {
                    var assignment = assignmentDocumentsStorage.GetById(calendarEvent.AssignmentId);
                    assignment.CalendarEvent = null;
                    assignment.CalendarEventId = null;
                    assignment.CalendarEventTimezoneId = null;
                    assignment.CalendarEventComment = null;
                    assignment.CalendarEventLastUpdate = null;
                    assignmentDocumentsStorage.Store(assignment);
                }
            }

            calendarEventStorage.Remove(id);
            eventStorage.RemoveEventSourceById(id);
        }
    }
}