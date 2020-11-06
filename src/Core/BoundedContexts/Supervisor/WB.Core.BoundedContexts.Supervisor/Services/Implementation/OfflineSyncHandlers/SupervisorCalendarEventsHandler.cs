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
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;

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

        public SupervisorCalendarEventsHandler(ICalendarEventStorage calendarEventStorage,
            IEnumeratorEventStorage eventStore,
            ICommandService commandService,
            ILogger logger,
            IJsonAllTypesSerializer serializer,
            ILiteEventBus eventBus)
        {
            this.calendarEventStorage = calendarEventStorage;
            this.eventStore = eventStore;
            this.commandService = commandService;
            this.logger = logger;
            this.serializer = serializer;
            this.eventBus = eventBus;
        }

        public void Register(IRequestHandler requestHandler)
        {
            requestHandler.RegisterHandler<PostCalendarEventsRequest, OkResponse>(Handle);
            requestHandler.RegisterHandler<GetCalendarEventsRequest, GetCalendarEventsResponse>(GetCalendarEvents);
            requestHandler.RegisterHandler<GetCalendarEventDetailsRequest, GetCalendarEventDetailsResponse>(Handle);
            requestHandler.RegisterHandler<UploadCalendarEventRequest, OkResponse>(UploadCalendarEvent);
        }

        private Task<GetCalendarEventDetailsResponse> Handle(GetCalendarEventDetailsRequest arg)
        {
            var events = this.eventStore.Read(arg.CalendarEventId, 0).ToList();

            return Task.FromResult(new GetCalendarEventDetailsResponse
            {
                Events = events
            });
        }

        private Task<OkResponse> Handle(PostCalendarEventsRequest arg)
        {
            eventBus.PublishCommittedEvents(arg.Events);
            eventStore.StoreEvents(new CommittedEventStream(arg.CalendarEventId, arg.Events));

            return Task.FromResult(new OkResponse());
        }

        private Task<GetCalendarEventsResponse> GetCalendarEvents(GetCalendarEventsRequest arg)
        {
            var calendarEvents = calendarEventStorage.GetNotSynchedCalendarEvents(arg.UserId);

            List<CalendarEventApiView> response = calendarEvents.Select(x => new CalendarEventApiView
            {
                Id = x.Id,
                // IsRejected = x.Status == InterviewStatus.RejectedBySupervisor,
                // QuestionnaireIdentity = QuestionnaireIdentity.Parse(x.QuestionnaireId),
                // ResponsibleId = x.ResponsibleId,
                // IsMarkedAsReceivedByInterviewer = x.ReceivedByInterviewerAtUtc != null,
                // Sequence = eventStore.GetMaxSequenceForAnyEvent(x.Id, EventsThatAssignInterviewToResponsibleProvider.GetTypeNames())
            }).ToList();

            return Task.FromResult(new GetCalendarEventsResponse()
            {
                CalendarEvents = response
            });
        }
        
        private Task<OkResponse> UploadCalendarEvent(UploadCalendarEventRequest request)
        {
            var calendarEvent = request.CalendarEvent;

            this.logger.Info($"Uploading of calendar event {calendarEvent.CalendarEventId} started.");

            var innerwatch = Stopwatch.StartNew();

            try
            {
                var aggregateRootEvents = this.serializer.Deserialize<AggregateRootEvent[]>(calendarEvent.Events);

                var firstEvent = aggregateRootEvents.FirstOrDefault();

                if (firstEvent != null && eventStore.HasEventsAfterSpecifiedSequenceWithAnyOfSpecifiedTypes(firstEvent.EventSequence - 1,
                    calendarEvent.CalendarEventId, 
                    EventsThatChangeAnswersStateProvider.GetTypeNames()))
                {
                    throw new InterviewException("Provided interview package is outdated. New answers were given to the interview while interviewer had interview on a tablet", 
                        InterviewDomainExceptionType.PackageIsOudated);
                }

                AssertPackageNotDuplicated(aggregateRootEvents);

                var serializedEvents = aggregateRootEvents
                    .Select(e => e.Payload)
                    .ToArray();

                this.logger.Debug($"Interview events by {calendarEvent.CalendarEventId} deserialized. Took {innerwatch.Elapsed:g}.");
                innerwatch.Restart();

                bool shouldChangeSupervisorId = CheckIfInterviewerWasMovedToAnotherTeam(serializedEvents, out Guid? newSupervisorId);

                if (shouldChangeSupervisorId && !newSupervisorId.HasValue)
                    throw new InterviewException("Can't move interview to a new team, because supervisor id is empty",
                        exceptionType: InterviewDomainExceptionType.CantMoveToUndefinedTeam);

                this.commandService.Execute(new SynchronizeInterviewEventsCommand(
                    interviewId: calendarEvent.CalendarEventId,
                    userId: calendarEvent.MetaInfo.ResponsibleId,
                    questionnaireId: calendarEvent.MetaInfo.TemplateId,
                    questionnaireVersion: calendarEvent.MetaInfo.TemplateVersion,
                    createdOnClient: calendarEvent.MetaInfo.CreatedOnClient ?? false,
                    interviewStatus: (InterviewStatus) calendarEvent.MetaInfo.Status,
                    interviewKey: InterviewKey.Parse(request.InterviewKey),
                    synchronizedEvents: aggregateRootEvents,
                    newSupervisorId: shouldChangeSupervisorId ? newSupervisorId : null
                ), null);

                RecordProcessedPackageInfo(aggregateRootEvents);
                UpdateAssignmentQuantityByInterview(calendarEvent.InterviewId);
            }
            catch (Exception exception)
            {
                this.logger.Error($"Interview events by {calendarEvent.InterviewId} processing failed. Reason: '{exception.Message}'", exception);

                var interviewException = exception as InterviewException;
                interviewException = interviewException?.UnwrapAllInnerExceptions()
                    .OfType<InterviewException>()
                    .FirstOrDefault();

                if (interviewException != null && interviewException.ExceptionType == InterviewDomainExceptionType.QuestionnaireIsMissing)
                {
                    throw;
                }
                var exceptionType = interviewException?.ExceptionType.ToString() ?? UnknownExceptionType;

                this.brokenInterviewPackageStorage.Store(new BrokenInterviewPackageView
                {
                    InterviewId = calendarEvent.InterviewId,
                    InterviewKey = request.InterviewKey,
                    QuestionnaireId = calendarEvent.MetaInfo.TemplateId,
                    QuestionnaireVersion = calendarEvent.MetaInfo.TemplateVersion,
                    InterviewStatus = (InterviewStatus)calendarEvent.MetaInfo.Status,
                    ResponsibleId = calendarEvent.MetaInfo.ResponsibleId,
                    IncomingDate = DateTime.UtcNow,
                    Events = calendarEvent.Events,
                    PackageSize = calendarEvent.Events?.Length ?? 0,
                    ProcessingDate = DateTime.UtcNow,
                    ExceptionType = exceptionType,
                    ExceptionMessage = exception.Message,
                    ExceptionStackTrace = string.Join(Environment.NewLine, exception.UnwrapAllInnerExceptions().Select(ex => $"{ex.Message} {ex.StackTrace}"))
                });

                this.logger.Debug($"Calendar events by {calendarEvent.CalendarEventId} moved to broken packages. Took {innerwatch.Elapsed:g}.");
                innerwatch.Restart();
            }

            innerwatch.Stop();

            return Task.FromResult(new OkResponse());
        }
    }
}