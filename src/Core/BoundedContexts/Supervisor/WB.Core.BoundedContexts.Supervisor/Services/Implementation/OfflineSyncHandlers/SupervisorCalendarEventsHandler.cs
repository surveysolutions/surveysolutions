#nullable enable

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
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
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
            var interviewsOnUser = this.interviewViewRepository.Where(x =>
                (x.Status == InterviewStatus.RejectedBySupervisor ||
                 x.Status == InterviewStatus.InterviewerAssigned ||
                 x.Status == InterviewStatus.Restarted)
                && x.ResponsibleId == arg.UserId);
            var calendarEventsFormInterviews = interviewsOnUser.Where(x =>
                x.CalendarEventId.HasValue)
                .Select(i => i.CalendarEventId!.Value);
            var calendarEventsFormAssignments = this.assignmentStorage
                .LoadAll(arg.UserId)
                .Where(x => x.CalendarEventId.HasValue)
                .Select(x => x.CalendarEventId!.Value);
            var calendarEventsWithoutInterviews = calendarEventStorage.GetCalendarEventsForUser(arg.UserId)
                .Where(ce => ce.InterviewId.HasValue 
                     && this.interviewViewRepository.GetById(ce.InterviewId.Value.FormatGuid()) == null)
                .Select(ce => ce.Id);
            

            var calendarEventIds = calendarEventsFormInterviews
                .Union(calendarEventsFormAssignments)
                .Union(calendarEventsWithoutInterviews);
            
            List<CalendarEventApiView> response = calendarEventIds
                .Select(id => new CalendarEventApiView
            {
                CalendarEventId = id,
                Sequence = eventStore.GetLastEventSequence(id),
                //LastEventId = x.LastEventId,
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
                
                bool deleteCalendarEventAfterApplying = false, 
                    restoreCalendarEventBefore = false, 
                    restoreCalendarEventAfter = false, 
                    shouldRestorePreviousStateAfterApplying  = false;

                var responsibleId = request.InterviewerId;

                var activeCalendarEvent = request.CalendarEvent.MetaInfo.InterviewId.HasValue
                    ? calendarEventStorage.GetCalendarEventForInterview(request.CalendarEvent.MetaInfo.InterviewId.Value)
                    : calendarEventStorage.GetCalendarEventForAssigment(request.CalendarEvent.MetaInfo.AssignmentId);

                //check responsible
                //ignore calendar event event if responsible is another person
                var currentResponsibleId = GetResponsibleForCalendarEventEntity(request);
                if (currentResponsibleId != null && currentResponsibleId.Value != responsibleId)
                {
                    logger.Error($"Ignored events by calendar event {request.CalendarEvent.CalendarEventId} from {responsibleId} because current responsible {currentResponsibleId.Value}");
                    return Task.FromResult(new OkResponse());
                }
                
                //remove other older CE 
                if (activeCalendarEvent != null &&
                    activeCalendarEvent.Id != request.CalendarEvent.CalendarEventId
                    && activeCalendarEvent.LastUpdateDateUtc < request.CalendarEvent.MetaInfo.LastUpdateDateTime
                    && !deleteCalendarEventAfterApplying)
                {
                    commandService.Execute(new DeleteCalendarEventCommand(activeCalendarEvent.Id, 
                        responsibleId,
                        new QuestionnaireIdentity( ) //dummy
                        ));
                }
                
                var calendarEvent = calendarEventStorage.GetById(request.CalendarEvent.CalendarEventId);

                //if CE was deleted on server before last change on tablet - restore it
                //if it was deleted after - leave it deleted
                //could be deleted again later
                if(calendarEvent != null 
                   && !request.CalendarEvent.MetaInfo.IsDeleted 
                   && calendarEvent.IsDeleted 
                   && calendarEvent.LastUpdateDateUtc < request.CalendarEvent.MetaInfo.LastUpdateDateTime)
                        restoreCalendarEventBefore = true;

                if(calendarEvent != null 
                   && request.CalendarEvent.MetaInfo.IsDeleted 
                   && !calendarEvent.IsDeleted 
                   && calendarEvent.LastUpdateDateUtc > request.CalendarEvent.MetaInfo.LastUpdateDateTime)
                        restoreCalendarEventAfter = true;

                if (calendarEvent != null
                    && calendarEvent.LastUpdateDateUtc > request.CalendarEvent.MetaInfo.LastUpdateDateTime)
                    shouldRestorePreviousStateAfterApplying = true;
                
                var aggregateRootEvents = calendarEventStream.Select(c => new AggregateRootEvent(c)).ToArray();
                commandService.Execute(
                        new SyncCalendarEventEventsCommand(aggregateRootEvents,
                            request.CalendarEvent.CalendarEventId, responsibleId,
                            restoreCalendarEventBefore: restoreCalendarEventBefore,
                            restoreCalendarEventAfter: restoreCalendarEventAfter, 
                            deleteCalendarEventAfter: deleteCalendarEventAfterApplying,
                            shouldRestorePreviousStateAfterApplying,
                            new QuestionnaireIdentity() //dummy
                            ));
            
                RecordProcessedPackageInfo(calendarEventStream);
            }
            catch (Exception exception)
            {
                this.logger.Error($"Calendar event by {request.CalendarEvent.CalendarEventId} processing failed. Reason: '{exception.Message}'. Took {innerwatch.Elapsed:g}.", exception);
                innerwatch.Stop();
                throw;
            }

            innerwatch.Stop();

            return Task.FromResult(new OkResponse());
        }
        
        private Guid? GetResponsibleForCalendarEventEntity(UploadCalendarEventRequest request)
        {
            if (request.CalendarEvent.MetaInfo.InterviewId.HasValue)
            {
                var existingInterview = interviewViewRepository.GetById(request.CalendarEvent.MetaInfo.InterviewId.Value.FormatGuid());
                return existingInterview?.ResponsibleId;
            }
            else
            {
                var assignment = assignmentStorage.GetById(request.CalendarEvent.MetaInfo.AssignmentId);
                return assignment?.ResponsibleId;
            }
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
