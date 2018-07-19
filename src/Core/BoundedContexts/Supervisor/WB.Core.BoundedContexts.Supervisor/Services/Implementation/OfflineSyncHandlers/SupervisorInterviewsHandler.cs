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
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.Services.Implementation.OfflineSyncHandlers
{
    public class SupervisorInterviewsHandler : IHandleCommunicationMessage
    {
        public const string UnknownExceptionType = "Unexpected";

        private readonly ILiteEventBus eventBus;
        private readonly IEnumeratorEventStorage eventStore;
        private readonly IPlainStorage<InterviewView> interviews;
        private readonly IPlainStorage<SuperivsorReceivedPackageLogEntry, int> receivedPackagesLog;
        private readonly IJsonAllTypesSerializer serializer;
        private readonly ILogger logger;
        private readonly ICommandService commandService;
        private readonly IPlainStorage<BrokenInterviewPackageView, int?> brokenInterviewPackageStorage;
        private readonly IPrincipal principal;
        private readonly IPlainStorage<InterviewerDocument> interviewerViewRepository;
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
            IPlainStorage<InterviewerDocument> interviewerViewRepository,
            IAssignmentDocumentsStorage assignmentsStorage)
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
            this.interviewerViewRepository = interviewerViewRepository;
            this.assignmentsStorage = assignmentsStorage;
        }

        public void Register(IRequestHandler requestHandler)
        {
            requestHandler.RegisterHandler<CanSynchronizeRequest, CanSynchronizeResponse>(Handle);
            requestHandler.RegisterHandler<PostInterviewRequest, OkResponse>(Handle);
            requestHandler.RegisterHandler<GetInterviewsRequest, GetInterviewsResponse>(Handle);
            requestHandler.RegisterHandler<LogInterviewAsSuccessfullyHandledRequest, OkResponse>(Handle);
            requestHandler.RegisterHandler<GetInterviewDetailsRequest, GetInterviewDetailsResponse>(Handle);
            requestHandler.RegisterHandler<UploadInterviewRequest, OkResponse>(UploadInterview);
            requestHandler.RegisterHandler<SupervisorIdRequest, SupervisorIdResponse>(GetSupervisorId);
        }

        public Task<OkResponse> UploadInterview(UploadInterviewRequest request)
        {
            var interview = request.Interview;
           
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
                    synchronizedEvents: serializedEvents,
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

        private void UpdateAssignmentQuantityByInterview(Guid interviewId)
        {
            var interviewView = this.interviews.GetById(interviewId.FormatGuid());
            if (interviewView?.Assignment != null)
            {
                var assignment = this.assignmentsStorage.GetById(interviewView.Assignment.Value);
                assignment.CreatedInterviewsCount = this.interviews.Count(x => x.Assignment == interviewView.Assignment);
                this.assignmentsStorage.Store(assignment);
            }
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

        public Task<OkResponse> Handle(LogInterviewAsSuccessfullyHandledRequest arg)
        {
            return Task.FromResult(new OkResponse());
        }

        public Task<CanSynchronizeResponse> Handle(CanSynchronizeRequest arg)
        {
            var expectedVersion = ReflectionUtils.GetAssemblyVersion(typeof(SupervisorBoundedContextAssemblyIndicator));

            if (expectedVersion.Revision != arg.InterviewerBuildNumber)
            {
                return Task.FromResult(new CanSynchronizeResponse
                {
                    CanSyncronize = false,
                    Reason = SyncDeclineReason.UnexpectedClientVersion
                });
            }

            var user = interviewerViewRepository.GetById(arg.InterviewerId.FormatGuid());
            if (user == null)
            {
                return Task.FromResult(new CanSynchronizeResponse
                {
                    CanSyncronize = false,
                    Reason = SyncDeclineReason.NotATeamMember
                });
            }

            return Task.FromResult(new CanSynchronizeResponse
            {
                CanSyncronize = true
            });
        }

        private Task<SupervisorIdResponse> GetSupervisorId(SupervisorIdRequest request)
        {
            return Task.FromResult(new SupervisorIdResponse()
            {
                SupervisorId = principal.CurrentUserIdentity.UserId
            });
        }

        public Task<GetInterviewsResponse> Handle(GetInterviewsRequest arg)
        {
            var interviewsForUser = this.interviews.Where(x =>
                (x.Status == InterviewStatus.RejectedBySupervisor || x.Status == InterviewStatus.InterviewerAssigned)
                && x.ResponsibleId == arg.UserId);

            List<InterviewApiView> response = interviewsForUser.Select(x => new InterviewApiView
            {
                Id = x.InterviewId,
                IsRejected = x.Status == InterviewStatus.RejectedBySupervisor,
                QuestionnaireIdentity = QuestionnaireIdentity.Parse(x.QuestionnaireId)
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
