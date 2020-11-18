using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Events;
using Ncqrs.Eventing;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent;
using WB.Core.SharedKernels.DataCollection.Events;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.Services.Implementation.OfflineSyncHandlers
{
    public class SupervisorCalendarEventsHandler : IHandleCommunicationMessage
    {
        private readonly ICalendarEventStorage calendarEventStorage;
        private readonly IEnumeratorEventStorage eventStore;
        private readonly ICommandService commandService;
        private readonly ILogger logger;
        private readonly IJsonAllTypesSerializer serializer;
        private readonly ILiteEventBus eventBus;
        private readonly IPlainStorage<SuperivsorReceivedPackageLogEntry, int> receivedPackagesLog;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IAssignmentDocumentsStorage assignmentStorage;


        public SupervisorCalendarEventsHandler(ICalendarEventStorage calendarEventStorage,
            IEnumeratorEventStorage eventStore,
            ICommandService commandService,
            ILogger logger,
            IJsonAllTypesSerializer serializer,
            ILiteEventBus eventBus,
            IPlainStorage<SuperivsorReceivedPackageLogEntry, int> receivedPackagesLog,
            IPlainStorage<InterviewView> interviewViewRepository,
            IAssignmentDocumentsStorage assignmentStorage)
        {
            this.calendarEventStorage = calendarEventStorage;
            this.eventStore = eventStore;
            this.commandService = commandService;
            this.logger = logger;
            this.serializer = serializer;
            this.eventBus = eventBus;
            this.receivedPackagesLog = receivedPackagesLog;
            this.interviewViewRepository = interviewViewRepository;
            this.assignmentStorage = assignmentStorage;
        }

        public void Register(IRequestHandler requestHandler)
        {
            requestHandler.RegisterHandler<GetCalendarEventsRequest, GetCalendarEventsResponse>(GetCalendarEvents);
            requestHandler.RegisterHandler<GetCalendarEventDetailsRequest, GetCalendarEventDetailsResponse>(GetCalendarEventDetails);
            requestHandler.RegisterHandler<UploadCalendarEventRequest, OkResponse>(UploadCalendarEvent);
        }

        private Task<GetCalendarEventDetailsResponse> GetCalendarEventDetails(GetCalendarEventDetailsRequest arg)
        {
            var sequence = arg.Sequence + 1 ?? 0;
            var events = this.eventStore.Read(arg.CalendarEventId, sequence).ToList();

            return Task.FromResult(new GetCalendarEventDetailsResponse
            {
                Events = events
            });
        }

        private Task<GetCalendarEventsResponse> GetCalendarEvents(GetCalendarEventsRequest arg)
        {
            var calendarEvents = calendarEventStorage.GetCalendarEventsForUser(arg.UserId);

            List<CalendarEventApiView> response = calendarEvents.Select(x => new CalendarEventApiView
            {
                CalendarEventId = x.Id,
                Sequence = eventStore.GetLastEventSequence(x.Id),
                ResponsibleId = x.UserId,
                LastEventId = x.LastEventId,
                //IsMarkedAsReceivedByInterviewer = x.ReceivedByInterviewerAtUtc != null,
            }).ToList();

            return Task.FromResult(new GetCalendarEventsResponse()
            {
                CalendarEvents = response
            });
        }
        
        private Task<OkResponse> UploadCalendarEvent(UploadCalendarEventRequest request)
        {
            this.logger.Info($"Uploading of calendar event {request.CalendarEvent.CalendarEventId} started.");

            var innerwatch = Stopwatch.StartNew();

            try
            {
                var calendarEventStream = this.serializer.Deserialize<CommittedEvent[]>(request.CalendarEvent.Events);

                var isPackageDuplicated = IsPackageDuplicated(calendarEventStream);
                if (isPackageDuplicated)
                    return Task.FromResult(new OkResponse()); // ignore

                this.logger.Debug($"Calendar events by {request.CalendarEvent.CalendarEventId} deserialized. Took {innerwatch.Elapsed:g}.");
                innerwatch.Restart();

                var deleteCalendarEventAfterApplying = false;
                var activeCalendarEvent = request.CalendarEvent.MetaInfo.InterviewId.HasValue
                            ? calendarEventStorage.GetCalendarEventForInterview(request.CalendarEvent.MetaInfo.InterviewId.Value)
                            : calendarEventStorage.GetCalendarEventForAssigment(request.CalendarEvent.MetaInfo.AssignmentId);

                if (request.CalendarEvent.MetaInfo.InterviewId.HasValue)
                {
                    var existingInterview = interviewViewRepository.GetById(request.CalendarEvent.MetaInfo.InterviewId.Value.FormatGuid());
                    if (existingInterview != null 
                        && existingInterview.ResponsibleId != request.CalendarEvent.MetaInfo.ResponsibleId)
                        deleteCalendarEventAfterApplying = true;
                }
                else
                {
                    var assignment = assignmentStorage.GetById(request.CalendarEvent.MetaInfo.AssignmentId);
                    if (assignment != null 
                        && assignment.ResponsibleId != request.CalendarEvent.MetaInfo.ResponsibleId)
                        deleteCalendarEventAfterApplying = true;
                }

                //remove other older CE 
                if (activeCalendarEvent != null &&
                        activeCalendarEvent.Id != request.CalendarEvent.CalendarEventId
                        && activeCalendarEvent.LastUpdateDate < request.CalendarEvent.MetaInfo.LastUpdateDateTime
                        && !deleteCalendarEventAfterApplying)
                {
                    var deleteCommand = new DeleteCalendarEventCommand(activeCalendarEvent.Id,
                        request.CalendarEvent.MetaInfo.ResponsibleId);
                    commandService.Execute(deleteCommand);
                }
                
                var calendarEvent = calendarEventStorage.GetById(request.CalendarEvent.CalendarEventId);

                //if CE was deleted on server before last change on tablet - restore it
                //if it was deleted after - leave it deleted
                //could be deleted again later
                if(calendarEvent != null && calendarEvent.IsDeleted && calendarEvent.LastUpdateDate < request.CalendarEvent.MetaInfo.LastUpdateDateTime)
                    commandService.Execute(new RestoreCalendarEventCommand(
                        request.CalendarEvent.CalendarEventId, request.CalendarEvent.MetaInfo.ResponsibleId));

                var aggregateRootEvents = calendarEventStream.Select(c => new AggregateRootEvent(c)).ToArray();

                commandService.Execute(new SyncCalendarEventEventsCommand(aggregateRootEvents,
                            request.CalendarEvent.CalendarEventId,
                            request.CalendarEvent.MetaInfo.ResponsibleId));

                if (deleteCalendarEventAfterApplying)
                    commandService.Execute(new DeleteCalendarEventCommand(
                        request.CalendarEvent.CalendarEventId, request.CalendarEvent.MetaInfo.ResponsibleId));
     
                RecordProcessedPackageInfo(calendarEventStream);
            }
            catch (Exception exception)
            {
                this.logger.Error($"Calendar event by {request.CalendarEvent.CalendarEventId} processing failed. Reason: '{exception.Message}'", exception);
                innerwatch.Restart();
                throw;
            }

            innerwatch.Stop();

            return Task.FromResult(new OkResponse());
        }

        private bool IsPackageDuplicated(CommittedEvent[] events)
        {
            if (events.Length > 0)
            {
                var firstEvent = events[0];
                var lastEvent = events[^1];

                var existingReceivedPackageLog = this.receivedPackagesLog.Where(
                    x => x.FirstEventId == firstEvent.EventIdentifier &&
                         x.FirstEventTimestamp == firstEvent.EventTimeStamp &&
                         x.LastEventId == lastEvent.EventIdentifier &&
                         x.LastEventTimestamp == lastEvent.EventTimeStamp).Count;

                if (existingReceivedPackageLog > 0)
                {
                    return true; // Package already received and processed
                }
            }

            return false;
        }

        private void RecordProcessedPackageInfo(CommittedEvent[] events)
        {
            if (events.Length > 0)
            {
                var firstEvent = events[0];
                var lastEvent = events[^1];
                this.receivedPackagesLog.Store(new SuperivsorReceivedPackageLogEntry
                {
                    FirstEventId = firstEvent.EventIdentifier,
                    FirstEventTimestamp = firstEvent.EventTimeStamp,
                    LastEventId = lastEvent.EventIdentifier,
                    LastEventTimestamp = lastEvent.EventTimeStamp
                });
            }
        }
    }
}