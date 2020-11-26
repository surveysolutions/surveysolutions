#nullable enable
using System;
using System.Linq;
using Main.Core.Events;
using Microsoft.Extensions.Logging;
using Ncqrs.Eventing;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.CalendarEvents;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent;
using WB.Enumerator.Native.WebInterview;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization
{
    public class CalendarEventPackageService : ICalendarEventPackageService
    {
        private readonly ILogger<CalendarEventPackageService> logger;
        private readonly ICalendarEventService calendarEventService;
        private readonly ICommandService commandService;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewsLocal;
        private readonly IAssignmentsService assignmentsLocal;
        private readonly ISerializer serializer;

        public CalendarEventPackageService(ILogger<CalendarEventPackageService> logger,
            ICalendarEventService calendarEventService,
            ICommandService commandService,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewsLocal,
            IAssignmentsService assignmentsLocal,
            ISerializer serializer
            )
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.calendarEventService = calendarEventService;
            this.commandService = commandService;
            this.interviewsLocal = interviewsLocal;
            this.assignmentsLocal = assignmentsLocal;
            this.serializer = serializer;
        }

        public void ProcessPackage(CalendarEventPackage calendarEventPackage)
        {
            try
            {
                var deleteCalendarEventAfterApplying = false;
                var restoreCalendarEventBefore = false;
                var restoreCalendarEventAfter = false;
                

                var responsibleId = calendarEventPackage.ResponsibleId;

                var activeCalendarEventByInterviewOrAssignmentId = calendarEventPackage.InterviewId.HasValue
                    ? calendarEventService.GetActiveCalendarEventForInterviewId(calendarEventPackage.InterviewId.Value)
                    : calendarEventService.GetActiveCalendarEventForAssignmentId(calendarEventPackage.AssignmentId);

                //check responsible
                //ignore calendar event event if responsible is another person
                var currentResponsibleId = GetResponsibleForCalendarEventEntity(calendarEventPackage);
                if (currentResponsibleId != null && currentResponsibleId.Value != responsibleId)
                    deleteCalendarEventAfterApplying = true;

                //remove other older CE 
                if (activeCalendarEventByInterviewOrAssignmentId != null &&
                    activeCalendarEventByInterviewOrAssignmentId.PublicKey != calendarEventPackage.CalendarEventId
                    && activeCalendarEventByInterviewOrAssignmentId.UpdateDateUtc < calendarEventPackage.LastUpdateDateUtc
                    && !deleteCalendarEventAfterApplying)
                {
                    commandService.Execute(new DeleteCalendarEventCommand(
                            activeCalendarEventByInterviewOrAssignmentId.PublicKey,
                            responsibleId));
                }
                
                var calendarEventStream = serializer.Deserialize<CommittedEvent[]>(calendarEventPackage.Events);
                var aggregateRootEvents = calendarEventStream
                    .Select(c => new AggregateRootEvent(c)).ToArray();

                var calendarEvent = calendarEventService.GetCalendarEventById(calendarEventPackage.CalendarEventId);

                //if CE was deleted on server before last change on tablet - restore it
                //if it was deleted after - leave it deleted
                //could be deleted again later
                if(calendarEvent != null 
                   && !calendarEventPackage.IsDeleted 
                   && calendarEvent.DeletedAtUtc.HasValue 
                   && calendarEvent.UpdateDateUtc < calendarEventPackage.LastUpdateDateUtc)
                        restoreCalendarEventBefore = true;

                if(calendarEvent != null 
                   && calendarEventPackage.IsDeleted 
                   && calendarEvent.DeletedAtUtc == null 
                   && calendarEvent.UpdateDateUtc > calendarEventPackage.LastUpdateDateUtc)
                        restoreCalendarEventAfter = true;

                commandService.Execute(
                        new SyncCalendarEventEventsCommand(aggregateRootEvents,
                            calendarEventPackage.CalendarEventId, responsibleId,
                            restoreCalendarEventBefore: restoreCalendarEventBefore,
                            restoreCalendarEventAfter: restoreCalendarEventAfter, 
                            deleteCalendarEventAfter: deleteCalendarEventAfterApplying));
            }
            catch (Exception exception)
            {
                this.logger.LogError(
                    exception,
                    $"Calendar event events by {calendarEventPackage.CalendarEventId} processing failed. Reason: '{exception.Message}'");
                throw;
            }
        }

        private Guid? GetResponsibleForCalendarEventEntity(CalendarEventPackage calendarEventPackage)
        {
            if (calendarEventPackage.InterviewId.HasValue)
            {
                var existingInterview = interviewsLocal.GetById(calendarEventPackage.InterviewId.Value);
                return existingInterview?.ResponsibleId;
            }
            else
            {
                var assignment = assignmentsLocal.GetAssignment(calendarEventPackage.AssignmentId);
                return assignment?.ResponsibleId;
            }
        }
    }
}
