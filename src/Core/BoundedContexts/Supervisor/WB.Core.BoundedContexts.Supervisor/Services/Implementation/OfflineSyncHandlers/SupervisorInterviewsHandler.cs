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
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.Services.Implementation.OfflineSyncHandlers
{
    public class SupervisorInterviewsHandler : IHandleCommunicationMessage
    {
        public const string UnknownExceptionType = "Unexpected";

        private readonly ISupervisorSettings settings;
        private readonly ILiteEventBus eventBus;
        private readonly IEnumeratorEventStorage eventStore;
        private readonly IPlainStorage<InterviewView> interviews;
        private readonly IPlainStorage<SuperivsorReceivedPackageLogEntry, int> receivedPackagesLog;
        private readonly IJsonAllTypesSerializer serializer;
        private readonly ILogger logger;
        private readonly ICommandService commandService;
        private readonly IPlainStorage<BrokenInterviewPackageView, int?> brokenInterviewPackageStorage;
        private readonly IPrincipal principal;
        
        private readonly IAssignmentDocumentsStorage assignmentsStorage;
        
        public SupervisorInterviewsHandler(ILiteEventBus eventBus,
            IEnumeratorEventStorage eventStore,
            IPlainStorage<InterviewView> interviews,
            IJsonAllTypesSerializer serializer,
            ICommandService commandService,
            ILogger logger, 
            IPlainStorage<BrokenInterviewPackageView, int?> brokenInterviewPackageStorage,
            IPlainStorage<SuperivsorReceivedPackageLogEntry, int> receivedPackagesLog,
            IPrincipal principal,
            IAssignmentDocumentsStorage assignmentsStorage,
            ISupervisorSettings settings)
        {
            this.eventBus = eventBus;
            this.eventStore = eventStore;
            this.interviews = interviews;
            this.serializer = serializer;
            this.logger = logger;
            this.receivedPackagesLog = receivedPackagesLog;
            this.commandService = commandService;
            this.brokenInterviewPackageStorage = brokenInterviewPackageStorage;
            this.principal = principal;
            this.assignmentsStorage = assignmentsStorage;
            this.settings = settings;
        }

        public void Register(IRequestHandler requestHandler)
        {
            requestHandler.RegisterHandler<PostInterviewRequest, OkResponse>(Handle);
            requestHandler.RegisterHandler<GetInterviewsRequest, GetInterviewsResponse>(GetInterviews);
            requestHandler.RegisterHandler<LogInterviewAsSuccessfullyHandledRequest, OkResponse>(Handle);
            requestHandler.RegisterHandler<GetInterviewDetailsRequest, GetInterviewDetailsResponse>(Handle);
            requestHandler.RegisterHandler<GetInterviewDetailsAfterEventRequest, GetInterviewDetailsResponse>(Handle);
            requestHandler.RegisterHandler<UploadInterviewRequest, OkResponse>(UploadInterview);
            requestHandler.RegisterHandler<SupervisorIdRequest, SupervisorIdResponse>(GetSupervisorId);
            requestHandler.RegisterHandler<ApplicationSettingsRequest, ApplicationSettingsResponse>(GetApplicationSettings);
            requestHandler.RegisterHandler<GetInterviewSyncInfoPackageRequest, InterviewSyncInfoPackageResponse>(GetInterviewSyncInfoPackageRequest);
        }

        private Task<InterviewSyncInfoPackageResponse> GetInterviewSyncInfoPackageRequest(GetInterviewSyncInfoPackageRequest request)
        {
            var response = new InterviewSyncInfoPackageResponse()
            {
                SyncInfoPackageResponse = new SyncInfoPackageResponse()
            };
            
            var svEvents = eventStore.Read(request.InterviewId, 0).ToList();
            response.SyncInfoPackageResponse.HasInterview = svEvents.Count != 0;
            if (svEvents.Count == 0)
                return Task.FromResult(response);

            if (request.SyncInfoPackage.LastEventIdFromPreviousSync.HasValue)
                response.SyncInfoPackageResponse.NeedSendFullStream = svEvents.All(e => 
                    e.EventIdentifier != request.SyncInfoPackage.LastEventIdFromPreviousSync.Value);

            return Task.FromResult(response);
        }

        private Task<ApplicationSettingsResponse> GetApplicationSettings(ApplicationSettingsRequest arg)
        {
            return Task.FromResult(new ApplicationSettingsResponse()
            {
                NotificationsEnabled = settings.NotificationsEnabled
            });
        }

        public Task<OkResponse> UploadInterview(UploadInterviewRequest request)
        {
            var interview = request.Interview;

            this.logger.Info($"Uploading of interview {interview.InterviewId} started.");

            var innerwatch = Stopwatch.StartNew();

            try
            {
                var aggregateRootEvents = this.serializer.Deserialize<AggregateRootEvent[]>(interview.Events);

                var firstEvent = aggregateRootEvents.FirstOrDefault();

                if (firstEvent != null &&
                    firstEvent.Payload.GetType() != typeof(SynchronizationMetadataApplied) &&
                    eventStore.HasEventsAfterSpecifiedSequenceWithAnyOfSpecifiedTypes(firstEvent.EventSequence - 1,
                        interview.InterviewId, EventsThatChangeAnswersStateProvider.GetTypeNames()))
                {
                    throw new InterviewException("Provided interview package is outdated. New answers were given to the interview while interviewer had interview on a tablet", 
                        InterviewDomainExceptionType.PackageIsOudated);
                }

                AssertPackageNotDuplicated(aggregateRootEvents);

                if (request.Interview.IsFullEventStream)
                {
                    var svEvents = eventStore.Read(request.Interview.InterviewId, 0).ToList();
                    if (svEvents.Count > 0)
                        aggregateRootEvents = FilterDuplicateEvents(aggregateRootEvents, svEvents);
                }

                var serializedEvents = aggregateRootEvents
                    .Select(e => e.Payload)
                    .ToArray();

                this.logger.Debug($"Interview events by {interview.InterviewId} deserialized. Took {innerwatch.Elapsed:g}.");
                innerwatch.Restart();

                bool shouldChangeSupervisorId = CheckIfInterviewerWasMovedToAnotherTeam(serializedEvents, out Guid? newSupervisorId);

                if (shouldChangeSupervisorId && !newSupervisorId.HasValue)
                    throw new InterviewException("Can't move interview to a new team, because supervisor id is empty",
                        exceptionType: InterviewDomainExceptionType.CantMoveToUndefinedTeam);

                this.commandService.Execute(new SynchronizeInterviewEventsCommand(
                    interviewId: interview.InterviewId,
                    userId: interview.MetaInfo.ResponsibleId,
                    questionnaireId: interview.MetaInfo.TemplateId,
                    questionnaireVersion: interview.MetaInfo.TemplateVersion,
                    createdOnClient: interview.MetaInfo.CreatedOnClient ?? false,
                    interviewStatus: (InterviewStatus) interview.MetaInfo.Status,
                    interviewKey: InterviewKey.Parse(request.InterviewKey),
                    synchronizedEvents: aggregateRootEvents,
                    newSupervisorId: shouldChangeSupervisorId ? newSupervisorId : null
                ), null);

                RecordProcessedPackageInfo(aggregateRootEvents);
                UpdateAssignmentQuantityByInterview(interview.InterviewId);
            }
            catch (Exception exception)
            {
                this.logger.Error($"Interview events by {interview.InterviewId} processing failed. Reason: '{exception.Message}'", exception);

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
                    InterviewId = interview.InterviewId,
                    InterviewKey = request.InterviewKey,
                    QuestionnaireId = interview.MetaInfo.TemplateId,
                    QuestionnaireVersion = interview.MetaInfo.TemplateVersion,
                    InterviewStatus = (InterviewStatus)interview.MetaInfo.Status,
                    ResponsibleId = interview.MetaInfo.ResponsibleId,
                    IncomingDate = DateTime.UtcNow,
                    Events = interview.Events,
                    PackageSize = interview.Events?.Length ?? 0,
                    ProcessingDate = DateTime.UtcNow,
                    ExceptionType = exceptionType,
                    ExceptionMessage = exception.Message,
                    ExceptionStackTrace = string.Join(Environment.NewLine, exception.UnwrapAllInnerExceptions().Select(ex => $"{ex.Message} {ex.StackTrace}"))
                });

                this.logger.Debug($"Interview events by {interview.InterviewId} moved to broken packages. Took {innerwatch.Elapsed:g}.");
                innerwatch.Restart();
            }

            innerwatch.Stop();

            return Task.FromResult(new OkResponse());
        }
        
        private static HashSet<string> ChangeEventsState = EventsThatChangeAnswersStateProvider.GetTypeNames()
            .Concat(EventsThatAssignInterviewToResponsibleProvider.GetTypeNames())
            .ToHashSet();

        private AggregateRootEvent[] FilterDuplicateEvents(AggregateRootEvent[] tabletEvents, List<CommittedEvent> svEvents)
        {
            var tabletEventIds = tabletEvents.Select(e => e.EventIdentifier).Reverse();
            var hqEventIds = svEvents.Select(e => e.EventIdentifier).Reverse();
            var lastCommonEventId = tabletEventIds.Intersect(hqEventIds).FirstOrDefault();
            if (lastCommonEventId == default)
                return tabletEvents;
            
            var filteredHqEvents = tabletEvents.SkipWhile(e => e.EventIdentifier != lastCommonEventId).Skip(1).ToArray();
            if (filteredHqEvents.Any(e => ChangeEventsState.Contains(e.Payload.GetType().Name)))
            {
                throw new InterviewException(
                    "Found active event on supervisor side. Can not merge streams",
                    exceptionType: InterviewDomainExceptionType.InterviewHasIncompatibleMode);
            }

            var filteredTabletEvents = tabletEvents.SkipWhile(e => e.EventIdentifier != lastCommonEventId).Skip(1).ToArray();
            return filteredTabletEvents;
        }

        private void UpdateAssignmentQuantityByInterview(Guid interviewId)
        {
            var interviewView = this.interviews.GetById(interviewId.FormatGuid());

            if (interviewView?.Assignment == null) return;

            var assignment = this.assignmentsStorage.GetById(interviewView.Assignment.Value);
            if (assignment == null) return;

            assignment.CreatedInterviewsCount = this.interviews.Where(x => x.Assignment == interviewView.Assignment)
                .Count(x => x.FromHqSyncDateTime == null);

            this.assignmentsStorage.Store(assignment);
        }

        private bool CheckIfInterviewerWasMovedToAnotherTeam(
            WB.Core.Infrastructure.EventBus.IEvent[] interviewEvents, out Guid? newSupervisorId)
        {
            newSupervisorId = null;
            SupervisorAssigned supervisorAssigned = interviewEvents.OfType<SupervisorAssigned>().LastOrDefault();
            if (supervisorAssigned == null)
                return false;
            
            newSupervisorId = principal.CurrentUserIdentity.UserId;
            return newSupervisorId != supervisorAssigned.SupervisorId;
        }

        private void AssertPackageNotDuplicated(AggregateRootEvent[] aggregateRootEvents)
        {
            if (aggregateRootEvents.Length > 0)
            {
                var firstEvent = aggregateRootEvents[0];
                var lastEvent = aggregateRootEvents[aggregateRootEvents.Length - 1];

                var existingReceivedPackageLog = this.receivedPackagesLog.Where(
                    x => x.FirstEventId == firstEvent.EventIdentifier &&
                         x.FirstEventTimestamp == firstEvent.EventTimeStamp &&
                         x.LastEventId == lastEvent.EventIdentifier &&
                         x.LastEventTimestamp == lastEvent.EventTimeStamp).Count;

                if (existingReceivedPackageLog > 0)
                {
                    throw new InterviewException("Package already received and processed",
                        InterviewDomainExceptionType.DuplicateSyncPackage);
                }
            }
        }

        private void RecordProcessedPackageInfo(AggregateRootEvent[] aggregateRootEvents)
        {
            if (aggregateRootEvents.Length > 0)
            {
                this.receivedPackagesLog.Store(new SuperivsorReceivedPackageLogEntry
                {
                    FirstEventId = aggregateRootEvents[0].EventIdentifier,
                    FirstEventTimestamp = aggregateRootEvents[0].EventTimeStamp,
                    LastEventId = aggregateRootEvents.Last().EventIdentifier,
                    LastEventTimestamp = aggregateRootEvents.Last().EventTimeStamp
                });
            }
        }

        public Task<GetInterviewDetailsResponse> Handle(GetInterviewDetailsRequest arg)
        {
            var events = this.eventStore.Read(arg.InterviewId, 0).ToList();

            return Task.FromResult(new GetInterviewDetailsResponse
            {
                Events = events
            });
        }

        private Task<GetInterviewDetailsResponse> Handle(GetInterviewDetailsAfterEventRequest arg)
        {
            var events = this.eventStore.Read(arg.InterviewId, 0)
                .SkipWhile(e => e.EventIdentifier != arg.EventId)
                .Skip(1)
                .ToList();

            return Task.FromResult(new GetInterviewDetailsResponse
            {
                Events = events
            });
        }

        public Task<OkResponse> Handle(LogInterviewAsSuccessfullyHandledRequest arg)
        {
            InterviewView interview = this.interviews.GetById(arg.InterviewId.FormatGuid());
            interview.ReceivedByInterviewerAtUtc = DateTime.UtcNow;
            this.interviews.Store(interview);
            return Task.FromResult(new OkResponse());
        }


        private Task<SupervisorIdResponse> GetSupervisorId(SupervisorIdRequest request)
        {
            return Task.FromResult(new SupervisorIdResponse()
            {
                SupervisorId = principal.CurrentUserIdentity.UserId
            });
        }

        public Task<GetInterviewsResponse> GetInterviews(GetInterviewsRequest arg)
        {
            var interviewsForUser = this.interviews.Where(x =>
                (x.Status == InterviewStatus.RejectedBySupervisor || 
                 x.Status == InterviewStatus.InterviewerAssigned || 
                 x.Status == InterviewStatus.Restarted)
                && (x.Mode == null || x.Mode != InterviewMode.CAWI)
                && x.ResponsibleId == arg.UserId);

            List<InterviewApiView> response = interviewsForUser.Select(x => new InterviewApiView
            {
                Id = x.InterviewId,
                IsRejected = x.Status == InterviewStatus.RejectedBySupervisor,
                QuestionnaireIdentity = QuestionnaireIdentity.Parse(x.QuestionnaireId),
                ResponsibleId = x.ResponsibleId,
                IsMarkedAsReceivedByInterviewer = x.ReceivedByInterviewerAtUtc != null,
                Sequence = eventStore.GetMaxSequenceForAnyEvent(x.InterviewId, EventsThatAssignInterviewToResponsibleProvider.GetTypeNames())
            }).ToList();

            return Task.FromResult(new GetInterviewsResponse
            {
                Interviews = response
            });
        }

        public Task<OkResponse> Handle(PostInterviewRequest arg)
        {
            eventBus.PublishCommittedEvents(arg.Events);
            eventStore.StoreEvents(new CommittedEventStream(arg.InterviewId, arg.Events));

            return Task.FromResult(new OkResponse());
        }
    }
}
