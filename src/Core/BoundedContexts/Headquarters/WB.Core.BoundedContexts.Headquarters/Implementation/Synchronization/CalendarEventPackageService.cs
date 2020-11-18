using System;
using Main.Core.Events;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.CalendarEvents;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent;
using WB.Enumerator.Native.WebInterview;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization
{
    public class CalendarEventPackageService : ICalendarEventPackageService
    {
        private readonly ILogger logger;
        private readonly SyncSettings syncSettings;

        public CalendarEventPackageService(ILogger logger, SyncSettings syncSettings)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.syncSettings = syncSettings ?? throw new ArgumentNullException(nameof(syncSettings));
        }

        public void ProcessPackage(CalendarEventPackage calendarEventPackage)
        {
            //check responsible
            //ignore calendar event event if responsible is another person
            try
            {
                var deleteCalendarEventAfterApplying = false;
                InScopeExecutor.Current.Execute(serviceLocator =>
                {
                    var calendarEventService = serviceLocator.GetInstance<ICalendarEventService>();

                    var activeCalendarEventByInterviewOrAssignmentId = calendarEventPackage.InterviewId.HasValue
                        ? calendarEventService.GetActiveCalendarEventForInterviewId(calendarEventPackage.InterviewId.Value)
                        : calendarEventService.GetActiveCalendarEventForAssignmentId(calendarEventPackage.AssignmentId);

                    if (calendarEventPackage.InterviewId.HasValue)
                    {
                        var interviewsLocal = serviceLocator.GetInstance<IQueryableReadSideRepositoryReader<InterviewSummary>>();
                        var existingInterview = interviewsLocal.GetById(calendarEventPackage.InterviewId.Value);
                        if (existingInterview != null 
                            && existingInterview.ResponsibleId != calendarEventPackage.ResponsibleId)
                            deleteCalendarEventAfterApplying = true;
                    }
                    else
                    {
                        var assignmentsLocal = serviceLocator.GetInstance<IAssignmentsService>();
                        var assignment = assignmentsLocal.GetAssignment(calendarEventPackage.AssignmentId);
                        if (assignment != null 
                            && assignment.ResponsibleId != calendarEventPackage.ResponsibleId)
                            deleteCalendarEventAfterApplying = true;
                    }

                    //remove other older CE 
                    if (activeCalendarEventByInterviewOrAssignmentId != null &&
                            activeCalendarEventByInterviewOrAssignmentId.PublicKey != calendarEventPackage.CalendarEventId
                            && activeCalendarEventByInterviewOrAssignmentId.UpdateDate < calendarEventPackage.LastUpdateDate
                            && !deleteCalendarEventAfterApplying)
                    {
                            serviceLocator.GetInstance<ICommandService>().Execute(
                                new DeleteCalendarEventCommand(
                                    activeCalendarEventByInterviewOrAssignmentId.PublicKey,
                                    calendarEventPackage.ResponsibleId),
                                this.syncSettings.Origin);
                    }
                    
                    var aggregateRootEvents = serviceLocator.GetInstance<IJsonAllTypesSerializer>()
                            .Deserialize<AggregateRootEvent[]>(calendarEventPackage.Events.Replace(@"\u0000", ""));

                    var calendarEvent = calendarEventService.GetCalendarEventById(calendarEventPackage.CalendarEventId);

                    if (calendarEvent != null)
                    {
                        //if CE was deleted on server before last change on tablet - restore it
                        //if it was deleted after - leave it deleted
                        //could be deleted again later
                        if(calendarEvent.IsDeleted && calendarEvent.UpdateDate < calendarEventPackage.LastUpdateDate)
                            serviceLocator.GetInstance<ICommandService>().Execute(
                                new RestoreCalendarEventCommand(calendarEventPackage.CalendarEventId,
                                    calendarEventPackage.ResponsibleId));
                    }

                    serviceLocator.GetInstance<ICommandService>().Execute(
                            new SyncCalendarEventEventsCommand(aggregateRootEvents,
                                calendarEventPackage.CalendarEventId,
                                calendarEventPackage.ResponsibleId),
                            this.syncSettings.Origin);

                    if (deleteCalendarEventAfterApplying)
                        serviceLocator.GetInstance<ICommandService>().Execute(
                            new DeleteCalendarEventCommand(calendarEventPackage.CalendarEventId,
                                calendarEventPackage.ResponsibleId));

                });
            }
            catch (Exception exception)
            {
                this.logger.Error(
                    $"Calendar event events by {calendarEventPackage.CalendarEventId} processing failed. Reason: '{exception.Message}'",
                    exception);

                throw;
            }
        }
    }
}
