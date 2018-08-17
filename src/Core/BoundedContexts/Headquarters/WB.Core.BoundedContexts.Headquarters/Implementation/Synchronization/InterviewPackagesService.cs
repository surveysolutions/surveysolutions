using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Main.Core.Events;
using Ncqrs.Eventing.Storage;
using NHibernate;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Enumerator.Native.WebInterview;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization
{
    internal class InterviewPackagesService : IInterviewPackagesService, IInterviewBrokenPackagesService
    {
        public const string UnknownExceptionType = "Unexpected";
        private readonly IPlainStorageAccessor<InterviewPackage> interviewPackageStorage;
        private readonly IPlainStorageAccessor<BrokenInterviewPackage> brokenInterviewPackageStorage;
        private readonly IPlainStorageAccessor<ReceivedPackageLogEntry> packagesTracker;
        private readonly IHeadquartersEventStore eventStore;
        private readonly ILogger logger;
        private readonly IJsonAllTypesSerializer serializer;
        private readonly ICommandService commandService;
        private readonly IInterviewUniqueKeyGenerator uniqueKeyGenerator;
        private readonly SyncSettings syncSettings;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviews;
        private readonly IUserRepository userRepository;
        private readonly IUnitOfWork unitOfWork;

        public InterviewPackagesService(
            IPlainStorageAccessor<InterviewPackage> interviewPackageStorage,
            IPlainStorageAccessor<BrokenInterviewPackage> brokenInterviewPackageStorage,
            IPlainStorageAccessor<ReceivedPackageLogEntry> packagesTracker,
            IHeadquartersEventStore eventStore,
            ILogger logger,
            IJsonAllTypesSerializer serializer,
            ICommandService commandService,
            IInterviewUniqueKeyGenerator uniqueKeyGenerator,
            SyncSettings syncSettings,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviews,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork)
        {
            this.interviewPackageStorage = interviewPackageStorage;
            this.brokenInterviewPackageStorage = brokenInterviewPackageStorage;
            this.packagesTracker = packagesTracker;
            this.eventStore = eventStore;
            this.logger = logger;
            this.serializer = serializer;
            this.commandService = commandService;
            this.uniqueKeyGenerator = uniqueKeyGenerator;
            this.syncSettings = syncSettings;
            this.interviews = interviews;
            this.userRepository = userRepository;
            this.unitOfWork = unitOfWork;
        }

        public void StoreOrProcessPackage(InterviewPackage interviewPackage)
        {
            if (this.syncSettings.UseBackgroundJobForProcessingPackages)
            {
                this.interviewPackageStorage.Store(interviewPackage, null);
            }
            else
            {
                this.ProcessPackage(interviewPackage);
            }
        }

        public virtual int QueueLength
            => this.interviewPackageStorage.Query(packages => packages.Count());

        public int InvalidPackagesCount
            => this.brokenInterviewPackageStorage.Query(packages => packages.Count());

        public virtual bool HasPendingPackageByInterview(Guid interviewId)
            => this.interviewPackageStorage.Query(
                packages => packages.Any(package => package.InterviewId == interviewId));

        public void ReprocessSelectedBrokenPackages(int[] packageIds)
        {
            packageIds.ForEach(packageId =>
            {
                var brokenInterviewPackage = this.brokenInterviewPackageStorage.GetById(packageId);

                var interviewPackage = new InterviewPackage
                {
                    ResponsibleId = brokenInterviewPackage.ResponsibleId,
                    InterviewId = brokenInterviewPackage.InterviewId,
                    IncomingDate = brokenInterviewPackage.IncomingDate,
                    InterviewStatus = brokenInterviewPackage.InterviewStatus,
                    IsCensusInterview = brokenInterviewPackage.IsCensusInterview,
                    QuestionnaireId = brokenInterviewPackage.QuestionnaireId,
                    QuestionnaireVersion = brokenInterviewPackage.QuestionnaireVersion,
                    Events = brokenInterviewPackage.Events,
                    ProcessAttemptsCount = brokenInterviewPackage.ReprocessAttemptsCount + 1
                };

                if (this.syncSettings.UseBackgroundJobForProcessingPackages)
                {
                    this.interviewPackageStorage.Store(interviewPackage, null);
                }

                else
                {
                    this.ProcessPackage(interviewPackage);
                }

                this.brokenInterviewPackageStorage.Remove(packageId);
            });
        }

        public void PutReason(int[] packageIds, InterviewDomainExceptionType requestErrorType)
        {
            packageIds.ForEach(packageId =>
            {
                var brokenInterviewPackage = this.brokenInterviewPackageStorage.GetById(packageId);
                brokenInterviewPackage.ExceptionType = requestErrorType.ToString();
            });
        }

        public virtual IReadOnlyCollection<string> GetAllPackagesInterviewIds()
        {
            var count = int.MaxValue;
            return this.interviewPackageStorage
                .Query(packages => packages.Select(package => package.InterviewId).Take(count).ToList())
                .Select(id => id.FormatGuid())
                .ToReadOnlyCollection();
        }

        public bool IsNeedShowBrokenPackageNotificationForInterview(Guid interviewId)
        {
            var undefined = InterviewDomainExceptionType.Undefined.ToString();
            return this.brokenInterviewPackageStorage.Query(_ =>
                _.Any(p => (p.ExceptionType == UnknownExceptionType || p.ExceptionType == undefined)
                    && p.InterviewId == interviewId
                    && 
                    (
                        p.ReprocessAttemptsCount > 2 
                        || _.Count(d => d.InterviewId == interviewId && (d.ExceptionType == UnknownExceptionType || d.ExceptionType == undefined)) > 1
                    )
                )
            );
        }

        public IReadOnlyCollection<int> GetTopBrokenPackageIdsAllowedToReprocess(int count)
        {
            var utcNow = DateTime.UtcNow;
            var utcNow5Minutes = utcNow.AddMinutes(-5);
            var utcNow10Minutes = utcNow.AddMinutes(-10);
            var utcNow60Minutes = utcNow.AddMinutes(-60);
            var utcNow8Hours = utcNow.AddHours(-8);
            var utcNow24Hours = utcNow.AddHours(-24);
            var undefined = InterviewDomainExceptionType.Undefined.ToString();

            return this.brokenInterviewPackageStorage.Query(_ =>
            {
                var filteredByError = _.Where(p => (p.ExceptionType == UnknownExceptionType || p.ExceptionType == undefined)
                    && _.Count(d => d.InterviewId == p.InterviewId && (d.ExceptionType == UnknownExceptionType || d.ExceptionType == undefined)) == 1);

                var filteredByTries = filteredByError.Where(p => 
                    p.ProcessingDate < utcNow5Minutes && p.ReprocessAttemptsCount < 1
                    || p.ProcessingDate < utcNow10Minutes && p.ReprocessAttemptsCount < 2
                    || p.ProcessingDate < utcNow60Minutes && p.ReprocessAttemptsCount < 3
                    || p.ProcessingDate < utcNow8Hours && p.ReprocessAttemptsCount < 4
                    || p.ProcessingDate < utcNow24Hours && p.ReprocessAttemptsCount < 5
                ).OrderByDescending(p => p.ProcessingDate);
                return filteredByTries.Select(p => p.Id).Take(count);
            }).ToList();
        }

        public virtual IReadOnlyCollection<string> GetTopPackageIds(int count)
            => this.interviewPackageStorage
                .Query(packages => packages.Select(package => package.Id).Take(count).ToList())
                .Select(packageId => packageId.ToString())
                .ToReadOnlyCollection();

        public virtual void ProcessPackage(string sPackageId)
        {
            if (int.TryParse(sPackageId, out var packageId))
                this.ProcessPackage(packageId);
            else
                this.logger.Warn($"Package {sPackageId}. Unknown package id");
        }

        private void ProcessPackage(int packageId)
        {
            Stopwatch innerwatch = Stopwatch.StartNew();

            var package = this.interviewPackageStorage.GetById(packageId);

            this.logger.Debug($"Package {package.InterviewId} loaded from db. Took {innerwatch.Elapsed:g}.");
            innerwatch.Restart();

            this.ProcessPackage(package);
            this.interviewPackageStorage.Remove(packageId);

            this.logger.Debug($"Package {package.InterviewId} removed. Took {innerwatch.Elapsed:g}.");
            innerwatch.Stop();
        }

        public void ProcessPackage(InterviewPackage interview)
        {
            // TODO validate event stream versions for no gaps inside
            Stopwatch innerwatch = Stopwatch.StartNew();
            string existingInterviewKey = null;
            try
            {
                existingInterviewKey = this.interviews.GetById(interview.InterviewId)?.Key;
                var aggregateRootEvents = this.serializer
                    .Deserialize<AggregateRootEvent[]>(interview.Events.Replace(@"\u0000", ""));

                var firstEvent = aggregateRootEvents.FirstOrDefault();
                if (firstEvent != null && 
                    firstEvent.Payload.GetType() != typeof(SynchronizationMetadataApplied) &&
                    eventStore.HasEventsAfterSpecifiedSequenceWithAnyOfSpecifiedTypes(firstEvent.EventSequence - 1, 
                        interview.InterviewId,
                        EventsThatChangeAnswersStateProvider.GetTypeNames()))
                {
                    throw new InterviewException("Provided interview package is outdated. New answers were given to the interview while interviewer had interview on a tablet", InterviewDomainExceptionType.PackageIsOudated);
                }

                AssertPackageNotDuplicated(aggregateRootEvents);

                var serializedEvents = aggregateRootEvents
                    .Select(e => e.Payload)
                    .ToArray();

                this.logger.Debug($"Interview events by {interview.InterviewId} deserialized. Took {innerwatch.Elapsed:g}.");
                innerwatch.Restart();

                bool shouldChangeInterviewKey = CheckIfInterviewKeyNeedsToBeChanged(interview.InterviewId, serializedEvents);
                bool shouldChangeSupervisorId = CheckIfInterviewerWasMovedToAnotherTeam(interview.ResponsibleId,
                    serializedEvents, out Guid? newSupervisorId);

                if (shouldChangeSupervisorId && !newSupervisorId.HasValue)
                    throw new InterviewException("Can't move interview to a new team, because supervisor id is empty",
                        exceptionType: InterviewDomainExceptionType.CantMoveToUndefinedTeam);

                this.commandService.Execute(new SynchronizeInterviewEventsCommand(
                    interviewId: interview.InterviewId,
                    userId: interview.ResponsibleId,
                    questionnaireId: interview.QuestionnaireId,
                    questionnaireVersion: interview.QuestionnaireVersion,
                    interviewStatus: interview.InterviewStatus,
                    createdOnClient: interview.IsCensusInterview,
                    interviewKey: shouldChangeInterviewKey ? this.uniqueKeyGenerator.Get() : null,
                    synchronizedEvents: serializedEvents,
                    newSupervisorId: shouldChangeSupervisorId ? newSupervisorId : null
                ), this.syncSettings.Origin);

                RecordProcessedPackageInfo(aggregateRootEvents);

            }
            catch (Exception exception)
            {
                this.unitOfWork.Dispose();


                this.logger.Error($"Interview events by {interview.InterviewId} processing failed. Reason: '{exception.Message}'", exception);

                var interviewException = exception as InterviewException;
                if (interviewException == null)
                {
                    interviewException = exception.UnwrapAllInnerExceptions()
                        .OfType<InterviewException>()
                        .FirstOrDefault();
                }

                var exceptionType = interviewException?.ExceptionType.ToString() ?? UnknownExceptionType;

                using (var brokenPackageUow = new UnitOfWork(ServiceLocator.Current.GetInstance<ISessionFactory>()))
                {
                    brokenPackageUow.Session.Save(new BrokenInterviewPackage
                    {
                        InterviewId = interview.InterviewId,
                        InterviewKey = existingInterviewKey,
                        QuestionnaireId = interview.QuestionnaireId,
                        QuestionnaireVersion = interview.QuestionnaireVersion,
                        InterviewStatus = interview.InterviewStatus,
                        ResponsibleId = interview.ResponsibleId,
                        IsCensusInterview = interview.IsCensusInterview,
                        IncomingDate = interview.IncomingDate,
                        Events = interview.Events,
                        PackageSize = interview.Events?.Length ?? 0,
                        ProcessingDate = DateTime.UtcNow,
                        ExceptionType = exceptionType,
                        ExceptionMessage = exception.Message,
                        ExceptionStackTrace = string.Join(Environment.NewLine, exception.UnwrapAllInnerExceptions().Select(ex => $"{ex.Message} {ex.StackTrace}")),
                        ReprocessAttemptsCount = interview.ProcessAttemptsCount,
                    });
                    brokenPackageUow.AcceptChanges();
                }

                this.logger.Debug($"Interview events by {interview.InterviewId} moved to broken packages. Took {innerwatch.Elapsed:g}.");
                innerwatch.Restart();
            }

            innerwatch.Stop();
        }

        private void RecordProcessedPackageInfo(AggregateRootEvent[] aggregateRootEvents)
        {
            if (aggregateRootEvents.Length > 0)
            {
                this.packagesTracker.Store(new ReceivedPackageLogEntry
                {
                    FirstEventId = aggregateRootEvents[0].EventIdentifier,
                    FirstEventTimestamp = aggregateRootEvents[0].EventTimeStamp,
                    LastEventId = aggregateRootEvents.Last().EventIdentifier,
                    LastEventTimestamp = aggregateRootEvents.Last().EventTimeStamp
                }, null);
            }
        }

        private void AssertPackageNotDuplicated(AggregateRootEvent[] aggregateRootEvents)
        {
            if (aggregateRootEvents.Length > 0)
            {
                var firstEvent = aggregateRootEvents[0];
                var lastEvent = aggregateRootEvents[aggregateRootEvents.Length - 1];

                var existingReceivedPackageLog = this.packagesTracker.Query(_ =>
                    _.FirstOrDefault(x => x.FirstEventId == firstEvent.EventIdentifier &&
                                          x.FirstEventTimestamp == firstEvent.EventTimeStamp &&
                                          x.LastEventId == lastEvent.EventIdentifier &&
                                          x.LastEventTimestamp == lastEvent.EventTimeStamp));

                if (existingReceivedPackageLog != null)
                {
                    throw new InterviewException("Package already received and processed",
                        InterviewDomainExceptionType.DuplicateSyncPackage);
                }
            }
        }

        private bool CheckIfInterviewerWasMovedToAnotherTeam(Guid responsibleId,
            IEvent[] interviewEvents, out Guid? newSupervisorId)
        {
            newSupervisorId = null;
            SupervisorAssigned supervisorAssigned = interviewEvents.OfType<SupervisorAssigned>().LastOrDefault();
            if (supervisorAssigned == null)
                return false;
            HqUser interviewer = userRepository.FindByIdAsync(responsibleId).Result;
            newSupervisorId = interviewer.Profile.SupervisorId;
            return newSupervisorId != supervisorAssigned.SupervisorId;
        }

        private bool CheckIfInterviewKeyNeedsToBeChanged(Guid interviewId, IEvent[] interviewEvents)
        {
            InterviewKeyAssigned interviewKeyEvent = interviewEvents.OfType<InterviewKeyAssigned>().LastOrDefault();
            if (interviewKeyEvent != null)
            {
                var stringKey = interviewKeyEvent.Key.ToString();
                var existingInterview = this.interviews.Query(
                        _ => _.FirstOrDefault(x => x.Key == stringKey && x.InterviewId != interviewId));

                if (existingInterview != null)
                {
                    return true;
                }

                return false;
            }

            var interview = this.interviews.Query(_ => _.Where(x => x.SummaryId == interviewId.FormatGuid())
                .Select(x => x.Key).FirstOrDefault());
            return interview == null;
        }
    }
}
