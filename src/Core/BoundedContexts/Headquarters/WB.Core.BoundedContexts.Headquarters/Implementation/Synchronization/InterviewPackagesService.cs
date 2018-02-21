using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Events;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Infrastructure.Native.Monitoring;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization
{
    internal class InterviewPackagesService : IInterviewPackagesService
    {
        public const string UnknownExceptionType = "Unexpected";
        private readonly IPlainStorageAccessor<InterviewPackage> interviewPackageStorage;
        private readonly IPlainStorageAccessor<BrokenInterviewPackage> brokenInterviewPackageStorage;
        private readonly ILogger logger;
        private readonly IJsonAllTypesSerializer serializer;
        private readonly ICommandService commandService;
        private readonly IInterviewUniqueKeyGenerator uniqueKeyGenerator;
        private readonly SyncSettings syncSettings;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviews;
        private readonly ITransactionManager transactionManager;
        private readonly IUserRepository userRepository;

        public InterviewPackagesService(
            IPlainStorageAccessor<InterviewPackage> interviewPackageStorage,
            IPlainStorageAccessor<BrokenInterviewPackage> brokenInterviewPackageStorage,
            ILogger logger,
            IJsonAllTypesSerializer serializer,
            ICommandService commandService,
            IInterviewUniqueKeyGenerator uniqueKeyGenerator,
            SyncSettings syncSettings,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviews,
            ITransactionManager transactionManager,
            IUserRepository userRepository)
        {
            this.interviewPackageStorage = interviewPackageStorage;
            this.brokenInterviewPackageStorage = brokenInterviewPackageStorage;
            this.logger = logger;
            this.serializer = serializer;
            this.commandService = commandService;
            this.uniqueKeyGenerator = uniqueKeyGenerator;
            this.syncSettings = syncSettings;
            this.interviews = interviews;
            this.transactionManager = transactionManager;
            this.userRepository = userRepository;
        }

        [Obsolete("Since v 5.8")]
        public virtual void StoreOrProcessPackage(string item)
        {
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

                CommonMetrics.BrokenPackagesCount.Labels(brokenInterviewPackage.ExceptionType).Dec();
                this.brokenInterviewPackageStorage.Remove(packageId);
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

        public IReadOnlyCollection<int> GetTopBrokenPackageIdsAllowedToReprocess(int count)
        {
            var utcNow = DateTime.UtcNow;
            var utcNow5Minutes = utcNow.AddMinutes(-5);
            var utcNow10Minutes = utcNow.AddMinutes(-10);
            var utcNow60Minutes = utcNow.AddMinutes(-60);
            var utcNow8Hours = utcNow.AddHours(-8);
            var utcNow24Hours = utcNow.AddHours(-24);

            return this.brokenInterviewPackageStorage.Query(_ =>
            {
                var filteredByError = _.Where(p => p.ExceptionType == UnknownExceptionType
                    && _.Count(d => d.InterviewId == p.InterviewId) == 1);

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
            Stopwatch innerwatch = Stopwatch.StartNew();
            string existingInterviewKey = null;
            try
            {
                bool startedOwnTransaction = false;
                if (!this.transactionManager.TransactionStarted)
                {
                    startedOwnTransaction = true;
                    transactionManager.BeginCommandTransaction();
                }

                existingInterviewKey = this.interviews.GetById(interview.InterviewId)?.Key;
                var serializedEvents = this.serializer
                    .Deserialize<AggregateRootEvent[]>(interview.Events)
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

                if (startedOwnTransaction)
                {
                    this.transactionManager.CommitCommandTransaction();
                }
            }
            catch (Exception exception)
            {
                this.logger.Error($"Interview events by {interview.InterviewId} processing failed. Reason: '{exception.Message}'", exception);
                

                this.transactionManager.RollbackCommandTransaction();

                var exceptionType = (exception as InterviewException)?.ExceptionType.ToString() ?? UnknownExceptionType;

                this.brokenInterviewPackageStorage.Store(new BrokenInterviewPackage
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
                }, null);

                CommonMetrics.BrokenPackagesCount.Labels(exceptionType).Inc();

                this.logger.Debug($"Interview events by {interview.InterviewId} moved to broken packages. Took {innerwatch.Elapsed:g}.");
                innerwatch.Restart();
            }

            innerwatch.Stop();
        }

        private bool CheckIfInterviewerWasMovedToAnotherTeam(Guid interviewerId,
            IEvent[] interviewEvents, out Guid? newSupervisorId)
        {
            newSupervisorId = null;
            SupervisorAssigned supervisorAssigned = interviewEvents.OfType<SupervisorAssigned>().LastOrDefault();
            if (supervisorAssigned == null)
                return false;
            HqUser interviewer = userRepository.FindByIdAsync(interviewerId).Result;
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