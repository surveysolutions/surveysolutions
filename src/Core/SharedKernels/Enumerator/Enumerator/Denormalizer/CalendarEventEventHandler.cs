using System;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection.Events.CalendarEvent;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Denormalizer
{
    public class CalendarEventEventHandler : BaseDenormalizer, 
                                         IEventHandler<CalendarEventCreated>,
                                         IEventHandler<CalendarEventUpdated>,
                                         IEventHandler<CalendarEventCompleted>,
                                         IEventHandler<CalendarEventDeleted>
    {
        private readonly ICalendarEventStorage calendarEventStorage;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IAssignmentDocumentsStorage assignmentStorage;

        public CalendarEventEventHandler(ICalendarEventStorage calendarEventStorage,
            IPlainStorage<InterviewView> interviewViewRepository,
            IAssignmentDocumentsStorage assignmentStorage)
        {
            this.calendarEventStorage = calendarEventStorage;
            this.interviewViewRepository = interviewViewRepository;
            this.assignmentStorage = assignmentStorage;
        }

        public void Handle(IPublishedEvent<CalendarEventCreated> evnt)
        {
            CalendarEvent calendarEvent = new CalendarEvent()
            {
                Id = evnt.EventSourceId,
                AssignmentId = evnt.Payload.AssignmentId,
                InterviewId = evnt.Payload.InterviewId,
                InterviewKey = evnt.Payload.InterviewKey,
                Comment = evnt.Payload.Comment,
                Start = evnt.Payload.Start,
                StartTimezone = evnt.Payload.StartTimezone,
                IsCompleted = false,
                IsDeleted = false,
                IsSynchronized = false,
                LastUpdateDate = evnt.EventTimeStamp,
                UserId = evnt.Payload.UserId,
                LastEventId = evnt.EventIdentifier,
            };
            calendarEventStorage.Store(calendarEvent);

            UpdateDashboardItemIfNeed(calendarEvent);
        }

        public void Handle(IPublishedEvent<CalendarEventUpdated> evnt)
        {
            UpdateCalendarEvent(evnt,
                calendarEvent =>
                {
                    calendarEvent.Start = evnt.Payload.Start;
                    calendarEvent.Comment = evnt.Payload.Comment;
                    calendarEvent.UserId = evnt.Payload.UserId;
                });
        }

        public void Handle(IPublishedEvent<CalendarEventCompleted> evnt)
        {
            UpdateCalendarEvent(evnt,
                calendarEvent =>
                {
                    calendarEvent.IsCompleted = true;
                });
        }

        public void Handle(IPublishedEvent<CalendarEventDeleted> evnt)
        {
            UpdateCalendarEvent(evnt,
                calendarEvent =>
                {
                    calendarEvent.IsDeleted = true;
                });
        }
        
        private void UpdateCalendarEvent(IPublishableEvent evnt, Action<CalendarEvent> updater)
        {
            CalendarEvent calendarEvent = calendarEventStorage.GetById(evnt.EventSourceId);
            if (calendarEvent == null) return;

            updater.Invoke(calendarEvent);
            
            calendarEvent.LastEventId = evnt.EventIdentifier;
            calendarEvent.IsSynchronized = false;
            calendarEvent.LastUpdateDate = evnt.EventTimeStamp;
            calendarEventStorage.Store(calendarEvent);

            UpdateDashboardItemIfNeed(calendarEvent);
        }

        private void UpdateDashboardItemIfNeed(CalendarEvent calendarEvent)
        {
            bool shouldDisplayData = !calendarEvent.IsCompleted && !calendarEvent.IsDeleted;
            
            if (calendarEvent.InterviewId.HasValue)
            {
                InterviewView interviewView = this.interviewViewRepository.GetById(calendarEvent.InterviewId.Value.FormatGuid());
                if (interviewView == null || interviewView.CalendarEventLastUpdate > calendarEvent.LastUpdateDate)
                    return;

                interviewView.CalendarEvent = shouldDisplayData ? calendarEvent.Start : (DateTimeOffset?)null;
                interviewView.CalendarEventComment = shouldDisplayData ? calendarEvent.Comment : null;
                interviewView.CalendarEventLastUpdate = calendarEvent.LastUpdateDate;
                this.interviewViewRepository.Store(interviewView);
            }
            else
            {
                var assignment = assignmentStorage.GetById(calendarEvent.AssignmentId);
                if (assignment == null)
                    return;

                assignment.CalendarEvent = shouldDisplayData ? calendarEvent.Start : (DateTimeOffset?)null;
                assignment.CalendarEventComment = shouldDisplayData ? calendarEvent.Comment : null;
                assignment.CalendarEventLastUpdate = calendarEvent.LastUpdateDate;
                this.assignmentStorage.Store(assignment);
            }
        }
    }
}
