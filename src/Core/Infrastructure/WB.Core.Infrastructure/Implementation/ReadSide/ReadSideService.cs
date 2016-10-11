using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Domain;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;

using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;

namespace WB.Core.Infrastructure.Implementation.ReadSide
{
    public class ReadSideService : IReadSideAdministrationService
    {
        internal static int InstanceCount = 0;

        private int totalEventsToRebuildCount = 0;

        private int FailedEventsCount => errors.Count;

        private int processedEventsCount = 0;
        private int skippedEventsCount = 0;
        private DateTime? lastRebuildDate = null;
        private readonly Stopwatch republishStopwatch = new Stopwatch();

        private int maxAllowedFailedEvents = 100;

        private static readonly object RebuildAllViewsLockObject = new object();
        private static readonly object ErrorsLockObject = new object();

        private static bool areViewsBeingRebuiltNow = false;
        private static bool shouldStopViewsRebuilding = false;

        private static string statusMessage;
        private static List<Tuple<DateTime, string, Exception>> errors = new List<Tuple<DateTime, string, Exception>>();

        private readonly IStreamableEventStore eventStore;
        private readonly IEventDispatcher eventBus;
        private readonly ILogger logger;
        private readonly IPostgresReadSideBootstraper postgresReadSideBootstraper;
        private Dictionary<IEventHandler, Stopwatch> handlersWithStopwatches;
        private readonly ITransactionManagerProviderManager transactionManagerProviderManager;

        private IPlainTransactionManagerProvider plainTransactionManagerProvider => ServiceLocator.Current.GetInstance<IPlainTransactionManagerProvider>();

        private readonly ReadSideSettings settings;
        private readonly IReadSideKeyValueStorage<ReadSideVersion> readSideVersionStorage;
        private int? cachedReadSideDatabaseVersion = null as int?;

        static ReadSideService()
        {
            UpdateStatusMessage("No administration operations were performed so far.");
        }

        public ReadSideService(
            IStreamableEventStore eventStore,
            IEventDispatcher eventBus, 
            ILogger logger,
            IPostgresReadSideBootstraper postgresReadSideBootstraper, 
            ITransactionManagerProviderManager transactionManagerProviderManager,
            ReadSideSettings settings,
            IReadSideKeyValueStorage<ReadSideVersion> readSideVersionStorage)
        {
            if (InstanceCount > 0)
                throw new Exception(string.Format("Trying to create a new instance of {1} when following count of instances exists: {0}.", InstanceCount, typeof(ReadSideService).Name));

            Interlocked.Increment(ref InstanceCount);

            this.eventStore = eventStore;
            this.eventBus = eventBus;
            this.logger = logger;
            this.postgresReadSideBootstraper = postgresReadSideBootstraper;
            this.transactionManagerProviderManager = transactionManagerProviderManager;
            this.settings = settings;
            this.readSideVersionStorage = readSideVersionStorage;
        }

        #region IReadSideStatusService implementation

        public bool AreViewsBeingRebuiltNow()
        {
            return areViewsBeingRebuiltNow;
        }

        public bool IsReadSideOutdated()
        {
            if (this.AreViewsBeingRebuiltNow())
                return false;

            this.InitializeDatabaseVersionIfNeeded();

            return this.GetReadSideDatabaseVersion() != this.GetReadSideApplicationVersion();
        }

        public int GetReadSideApplicationVersion()
        {
            return this.settings.ReadSideVersion;
        }

        public int? GetReadSideDatabaseVersion()
        {
            if (this.AreViewsBeingRebuiltNow())
                return null as int?;

            if (this.cachedReadSideDatabaseVersion.HasValue)
                return this.cachedReadSideDatabaseVersion.Value;

            ReadSideVersion readSideDatabaseVersion = null;
            try
            {
                this.transactionManagerProviderManager.GetTransactionManager().BeginQueryTransaction();
                readSideDatabaseVersion = this.readSideVersionStorage.GetById(ReadSideVersion.IdOfCurrent);
                this.cachedReadSideDatabaseVersion = readSideDatabaseVersion?.Version;
            }
            /*catch (Exception)
            {
            }*/
            finally
            {
                this.transactionManagerProviderManager.GetTransactionManager().RollbackQueryTransaction();
            }

            return readSideDatabaseVersion?.Version;
        }

        #endregion // IReadSideStatusService implementation

        #region IReadSideAdministrationService implementation

        public void RebuildAllViewsAsync(int skipEvents)
        {
            skipEvents = Math.Max(0, skipEvents);

            new Task(() => this.RebuildAllViews(skipEvents)).Start();
        }

        public void RebuildViewsAsync(string[] handlerNames, int skipEvents)
        {
            new Task(() => this.RebuildViews(skipEvents, handlerNames)).Start();
        }

        public void RebuildViewForEventSourcesAsync(string[] handlerNames, Guid[] eventSourceIds)
        {
            new Task(() => this.RebuildViewsForEventSources(this.GetListOfEventHandlersForRebuild(handlerNames), eventSourceIds)).Start();
        }

        public void StopAllViewsRebuilding()
        {
            if (!areViewsBeingRebuiltNow)
                return;

            shouldStopViewsRebuilding = true;
        }

        public IEnumerable<ReadSideEventHandlerDescription> GetAllAvailableHandlers()
        {
            return
                this.eventBus.GetAllRegistredEventHandlers()
                    .Select(
                        h =>
                        new ReadSideEventHandlerDescription(h.Name, h.Readers.Select(CreateViewName).ToArray(),
                                                    h.Writers.Select(CreateViewName).ToArray(), h is IAtomicEventHandler))
                    .ToList();
        }

        private List<ReadSideDenormalizerStatistic> GetRebuildDenormalizerStatistics()
        {
            if(handlersWithStopwatches==null)
                return new List<ReadSideDenormalizerStatistic>();
            var currentStateOfDenormalizers =
                handlersWithStopwatches.OrderByDescending(h => h.Value.Elapsed.Ticks)
                    .Select(h => new {Name = h.Key.Name, Ticks = h.Value.Elapsed.Ticks});

            var timeSpentForAllDenormalizers = currentStateOfDenormalizers.Sum(x => x.Ticks);
            return
                currentStateOfDenormalizers.Select(h => new ReadSideDenormalizerStatistic(h.Name, new TimeSpan(h.Ticks), (int)Math.Round((double)(100 * h.Ticks) / timeSpentForAllDenormalizers)))
                    .ToList();
        }

        public ReadSideStatus GetRebuildStatus()
        {
            int republishedEventsCount = this.processedEventsCount - this.skippedEventsCount;

            TimeSpan republishTimeSpent = this.republishStopwatch.Elapsed;

            int speedInEventsPerMinute = (int)(
                republishTimeSpent.TotalSeconds == 0
                    ? 0
                    : 60L * republishedEventsCount / republishTimeSpent.TotalSeconds);

            TimeSpan estimatedTotalRepublishTime = TimeSpan.FromMilliseconds(
                republishedEventsCount == 0
                    ? 0
                    : republishTimeSpent.TotalMilliseconds / republishedEventsCount * this.totalEventsToRebuildCount);


            var criticalRebuildReadSideExceptions = errors.Where(error => IsCriticalException(error.Item3)).ToList();
            var exceptionsByEventHandlersWhichShouldBeIgnored = errors.Except(criticalRebuildReadSideExceptions).ToList();

            return new ReadSideStatus
            {
                IsRebuildRunning = this.AreViewsBeingRebuiltNow(),

                ReadSideApplicationVersion = this.GetReadSideApplicationVersion(),
                ReadSideDatabaseVersion = this.GetReadSideDatabaseVersion(),

                CurrentRebuildStatus = statusMessage,
                LastRebuildDate = this.lastRebuildDate,
                EventPublishingDetails = new ReadSideEventPublishingDetails
                {
                    ProcessedEvents = republishedEventsCount,
                    EstimatedTime = this.AreViewsBeingRebuiltNow() ? estimatedTotalRepublishTime : null as TimeSpan?,
                    FailedEvents = this.FailedEventsCount,
                    SkippedEvents = this.skippedEventsCount,
                    Speed = speedInEventsPerMinute,
                    TimeSpent = republishTimeSpent,
                    TotalEvents = this.totalEventsToRebuildCount,
                },
                StatusByRepositoryWriters = this.eventBus.GetAllRegistredEventHandlers()
                    .SelectMany(x => x.Writers.OfType<IReadSideStorage>())
                    .Distinct()
                    .Select(
                        writer =>
                            new ReadSideRepositoryWriterStatus()
                            {
                                WriterName = GetStorageEntityName(writer),
                                Status = writer.GetReadableStatus()
                            }),
                WarningEventHandlerErrors = ReverseList(exceptionsByEventHandlersWhichShouldBeIgnored)
                    .Take(10)
                    .Select(error => new ReadSideRepositoryWriterError
                    {
                        ErrorTime = error.Item1,
                        ErrorMessage = error.Item2,
                        InnerException = GetFullUnwrappedExceptionText(error.Item3)
                    }),
                RebuildErrors = ReverseList(criticalRebuildReadSideExceptions)
                    .Take(10)
                    .Select(error => new ReadSideRepositoryWriterError
                    {
                        ErrorTime = error.Item1,
                        ErrorMessage = error.Item2,
                        InnerException = GetFullUnwrappedExceptionText(error.Item3)
                    }),
                ReadSideDenormalizerStatistics = GetRebuildDenormalizerStatistics()
            };
        }

        private static bool IsCriticalException(Exception exception)
        {
            var eventHandlerException =  exception.GetSelfOrInnerAs<EventHandlerException>();

            return eventHandlerException == null || eventHandlerException.IsCritical;
        }

        private string CreateViewName(object storage)
        {
            var readSideStorage = storage as IReadSideStorage;

            if (readSideStorage != null)
                return GetStorageEntityName(readSideStorage);

            return storage.GetType().Name;
        }

        #endregion // IReadSideAdministrationService implementation

        private void RebuildViewsForEventSources(IEventHandler[] handlers, Guid[] eventSourceIds)
        {
            if (!areViewsBeingRebuiltNow)
            {
                lock (RebuildAllViewsLockObject)
                {
                    if (!areViewsBeingRebuiltNow)
                    {
                        this.RebuildViewsByEventSourcesImpl(eventSourceIds, handlers);
                    }
                }
            }
        }

        private void RebuildViews(int skipEvents, string[] handlerNames)
        {
            if (!areViewsBeingRebuiltNow)
            {
                lock (RebuildAllViewsLockObject)
                {
                    if (!areViewsBeingRebuiltNow)
                    {
                        var handlers = this.GetListOfEventHandlersForRebuild(handlerNames);
                        this.RebuildViewsImpl(skipEvents, handlers, true);
                    }
                }
            }
        }

        private void RebuildAllViews(int skipEvents)
        {
            if (!areViewsBeingRebuiltNow)
            {
                lock (RebuildAllViewsLockObject)
                {
                    if (!areViewsBeingRebuiltNow)
                    {
                        this.RebuildViewsImpl(skipEvents, this.eventBus.GetAllRegistredEventHandlers(), false);
                    }
                }
            }
        }

        private IEventHandler[] GetListOfEventHandlersForRebuild(string[] handlerNames)
        {
            if (handlerNames == null) return new IEventHandler[0];

            var allHandlers = this.eventBus.GetAllRegistredEventHandlers();
            return allHandlers.Where(handler=>handlerNames.Contains(handler.Name)).ToArray();
        }

        private void RebuildViewsByEventSourcesImpl(Guid[] eventSourceIds, IEventHandler[] handlers)
        {
            areViewsBeingRebuiltNow = true;

            errors.Clear();

            var atomicEventHandlers = handlers.OfType<IAtomicEventHandler>().ToArray();

            if (atomicEventHandlers.Length != handlers.Length)
            {
                var message =
                    "Not all handlers supports partial rebuild. Handlers which are not supporting partial rebuild are " +
                    string.Join(",", handlers.Where(h => !atomicEventHandlers.Contains(h)).Select(h => h.Name));

                UpdateStatusMessage(message);

                throw new Exception(message);
            }

            try
            {
                var transactionManager = this.transactionManagerProviderManager.GetTransactionManager();
                foreach (var atomicEventHandler in atomicEventHandlers)
                {
                    ThrowIfShouldStopViewsRebuilding();

                    var cleanerName = atomicEventHandler.Name;

                    foreach (var eventSourceId in eventSourceIds)
                    {
                        UpdateStatusMessage($"Cleaning views for {cleanerName} and event source {eventSourceId}");
                        try
                        {
                            transactionManager.BeginCommandTransaction();
                            atomicEventHandler.CleanWritersByEventSource(eventSourceId);
                            transactionManager.CommitCommandTransaction();
                        }
                        catch
                        {  
                            transactionManager.RollbackCommandTransaction();
                            throw;
                        }
                        UpdateStatusMessage($"Views for {cleanerName} and event source {eventSourceId} was cleaned.");
                    }
                }

                try
                {
                    EnableWritersCacheForHandlers(handlers);
                    this.transactionManagerProviderManager.PinRebuildReadSideTransactionManager();
                    this.plainTransactionManagerProvider.PinRebuildReadSideTransactionManager();
                    this.republishStopwatch.Restart();

                    foreach (var eventSourceId in eventSourceIds)
                    {
                        var eventsToPublish = this.eventStore.Read(eventSourceId, 0).ToList();
                        this.RepublishAllEvents(eventsToPublish, eventsToPublish.Count, handlers: handlers);
                    }
                }
                finally
                {
                    this.republishStopwatch.Stop();
                    this.transactionManagerProviderManager.UnpinTransactionManager();
                    this.DisableWritersCacheForHandlers(handlers);
                    this.plainTransactionManagerProvider.UnpinTransactionManager();
                }

                UpdateStatusMessage("Rebuild views by event sources succeeded.");
                logger.Info("Rebuild views by event sources succeeded.");
            }
            catch (OperationCanceledException exception)
            {
                UpdateStatusMessage(exception.Message);
                throw;
            }
            catch (Exception exception)
            {
                UpdateStatusMessage("Unexpected error occurred");
                this.SaveErrorForStatusReport("Unexpected error occurred", exception);
                throw;
            }
            finally
            {
                areViewsBeingRebuiltNow = false;
            }
        }

        private void RebuildViewsImpl(int skipEvents, IEventHandler[] handlers, bool isPartialRebuild)
        {
            try
            {
                areViewsBeingRebuiltNow = true;
                using (GlobalStopwatcher.Scope("Rebuild read side"))
                {
                    errors.Clear();

                    if (skipEvents == 0)
                    {
                        if (!isPartialRebuild && this.postgresReadSideBootstraper != null)
                            this.postgresReadSideBootstraper.ReCreateViewDatabase();

                        this.CleanUpWritersForHandlers(handlers);
                    }

                    if (!isPartialRebuild)
                    {
                        this.CleanReadSideVersion();
                    }

                    try
                    {
                        EnableWritersCacheForHandlers(handlers);
                        this.transactionManagerProviderManager.PinRebuildReadSideTransactionManager();
                        this.plainTransactionManagerProvider.PinRebuildReadSideTransactionManager();
                        this.republishStopwatch.Restart();

                        this.RepublishAllEvents(this.GetEventStream(skipEvents), this.eventStore.CountOfAllEvents(),
                            skipEventsCount: skipEvents, handlers: handlers);
                    }
                    finally
                    {
                        this.republishStopwatch.Stop();
                        this.transactionManagerProviderManager.UnpinTransactionManager();
                        this.DisableWritersCacheForHandlers(handlers);
                        this.plainTransactionManagerProvider.UnpinTransactionManager();
                    }

                    if (!isPartialRebuild)
                    {
                        this.StoreReadSideVersion();
                    }

                    var finishMessage = this.FailedEventsCount > 0
                        ? $"Rebuild {(isPartialRebuild ? "specific" : "all")} views finished with {this.FailedEventsCount} failed event(s)."
                        : $"Rebuild {(isPartialRebuild ? "specific" : "all")} views succeeded.";

                    UpdateStatusMessage(finishMessage);
                    logger.Info(finishMessage);
                }
            }
            catch (OperationCanceledException exception)
            {
                UpdateStatusMessage(exception.Message);
                throw;
            }
            catch (Exception exception)
            {
                UpdateStatusMessage("Unexpected error occurred");
                this.SaveErrorForStatusReport("Unexpected error occurred", exception);
                throw;
            }
            finally
            {
                areViewsBeingRebuiltNow = false;
                this.logger.Info(GlobalStopwatcher.GetMeasureDetails());
                GlobalStopwatcher.Reset();
            }
        }

        private void CleanReadSideVersion()
        {
            UpdateStatusMessage("Cleaning read side version");

            this.cachedReadSideDatabaseVersion = null;

            var readSideVersionStorageAsCleaner = this.readSideVersionStorage as IReadSideRepositoryCleaner;

            if (readSideVersionStorageAsCleaner != null)
            {
                try
                {
                    this.transactionManagerProviderManager.GetTransactionManager().BeginCommandTransaction();
                    readSideVersionStorageAsCleaner.Clear();
                    this.transactionManagerProviderManager.GetTransactionManager().CommitCommandTransaction();
                }
                catch
                {
                    this.transactionManagerProviderManager.GetTransactionManager().RollbackCommandTransaction();
                    throw;
                }
            }
            UpdateStatusMessage("Read side version cleaned");
        }

        private void StoreReadSideVersion()
        {
            UpdateStatusMessage("Storing read side version");

            this.cachedReadSideDatabaseVersion = null;

            try
            {
                this.transactionManagerProviderManager.GetTransactionManager().BeginCommandTransaction();
                this.readSideVersionStorage.Store(new ReadSideVersion(this.GetReadSideApplicationVersion()), ReadSideVersion.IdOfCurrent);
                this.transactionManagerProviderManager.GetTransactionManager().CommitCommandTransaction();
            }
            catch
            {
                this.transactionManagerProviderManager.GetTransactionManager().RollbackCommandTransaction();
                throw;
            }

            UpdateStatusMessage("Read side version stored");
        }

        private void InitializeDatabaseVersionIfNeeded()
        {
            if (this.GetReadSideDatabaseVersion() == null && this.IsWriteSideEmpty())
            {
                this.StoreReadSideVersion();
            }
        }

        private bool IsWriteSideEmpty() => this.eventStore.CountOfAllEvents() == 0;

        private IEnumerable<CommittedEvent> GetEventStream(int skipEventsCount)
        {
            if (skipEventsCount > 0)
            {
                UpdateStatusMessage($"Skipping {skipEventsCount} events.");
            }

            return this.GetEventStream().Skip(skipEventsCount);
        }

        private IEnumerable<CommittedEvent> GetEventStream()
        {
            var eventSourcesAndSequences = new Dictionary<Guid, long>();

            foreach (CommittedEvent committedEvent in this.eventStore.GetAllEvents())
            {
                EnsureEventSequenceIsCorrect(committedEvent, eventSourcesAndSequences);

                yield return committedEvent;
            }
        }

        private static void EnsureEventSequenceIsCorrect(CommittedEvent committedEvent, Dictionary<Guid, long> eventSourcesAndSequences)
        {
            if (eventSourcesAndSequences.ContainsKey(committedEvent.EventSourceId))
            {
                long lastEventSequence = eventSourcesAndSequences[committedEvent.EventSourceId];
                long expectedEventSequence = lastEventSequence + 1;

                if (committedEvent.EventSequence != expectedEventSequence)
                {
                    throw new InvalidDataException(string.Format(
                        "Event {0} {1} (event source {2}) appears in all events stream not when expected. Event sequence: {3}. Expected sequence: {4}.",
                        committedEvent.EventIdentifier.FormatGuid(), committedEvent.Payload.GetType().Name,
                        committedEvent.EventSourceId.FormatGuid(),
                        committedEvent.EventSequence, expectedEventSequence));
                }
            }

            eventSourcesAndSequences[committedEvent.EventSourceId] = committedEvent.EventSequence;
        }


        private void CleanUpWritersForHandlers(IEnumerable<IEventHandler> handlers)
        {
            var cleaners = handlers.SelectMany(x=>x.Writers.OfType<IReadSideRepositoryCleaner>())
                  .Distinct()
                  .ToArray();

            foreach (var readSideRepositoryCleaner in cleaners)
            {
                ThrowIfShouldStopViewsRebuilding();

                var cleanerName = this.CreateViewName(readSideRepositoryCleaner);

                UpdateStatusMessage($"Deleting views for {cleanerName}");

                try
                {
                    this.transactionManagerProviderManager.GetTransactionManager().BeginCommandTransaction();
                    readSideRepositoryCleaner.Clear();
                    this.transactionManagerProviderManager.GetTransactionManager().CommitCommandTransaction();
                }
                catch
                {
                    this.transactionManagerProviderManager.GetTransactionManager().RollbackCommandTransaction();
                    throw;
                }

                UpdateStatusMessage($"Views for {cleanerName} were deleted.");
            }
        }

        private static void EnableWritersCacheForHandlers(IEnumerable<IEventHandler> handlers)
        {
            UpdateStatusMessage("Enabling cache in repository writers.");

            var writers = handlers.SelectMany(x => x.Writers.OfType<ICacheableRepositoryWriter>())
               .Distinct()
               .ToArray();

            foreach (ICacheableRepositoryWriter writer in writers)
            {
                try
                {
                    writer.EnableCache();
                }
                catch
                {
                    UpdateStatusMessage($"Failed to enable cache for repository writer {GetStorageEntityName(writer)}.");
                }
            }

            UpdateStatusMessage("Cache in repository writers enabled.");
        }

        private void DisableWritersCacheForHandlers(IEnumerable<IEventHandler> handlers)
        {
            using (GlobalStopwatcher.Scope("Disable caches"))
            {
                this.logger.Info("Starting Disabling cache in repository writers.");

                UpdateStatusMessage("Disabling cache in repository writers.");
                
                var writers = handlers.SelectMany(x => x.Writers.OfType<ICacheableRepositoryWriter>())
                    .Distinct()
                    .ToArray();

                var entitiesInProgress = new ConcurrentDictionary<string, Unit>();
                var failedWriters = new ConcurrentDictionary<string, Unit>();

                writers.AsParallel().ForAll(writer =>
                {
                    var storageEntityName = GetStorageEntityName(writer);

                    using (GlobalStopwatcher.Scope("Disable cache", storageEntityName))
                    {
                        this.logger.Info($"Disabling cache for {storageEntityName}");

                        entitiesInProgress.TryAdd(storageEntityName, Unit.Value);
                        UpdateStatusMessage($"Disabling cache for {string.Join(", ", entitiesInProgress.Keys)}.");

                        try
                        {
                            this.transactionManagerProviderManager.GetTransactionManager().BeginCommandTransaction();
                            writer.DisableCache();
                            this.transactionManagerProviderManager.GetTransactionManager().CommitCommandTransaction();
                        }
                        catch (Exception exception)
                        {
                            failedWriters.TryAdd(storageEntityName, Unit.Value);
                            this.transactionManagerProviderManager.GetTransactionManager().RollbackCommandTransaction();
                            string message = $"Failed to disable cache and store data to repository for writer {storageEntityName}.";
                            this.SaveErrorForStatusReport(message, exception);
                        }

                        entitiesInProgress.TryRemove(storageEntityName);

                        UpdateStatusMessage(
                            failedWriters.Count > 0
                                ? $"Disabling cache in repository writer for entities {string.Join(", ", entitiesInProgress.Keys)}. Failed writers: {string.Join(", ", failedWriters.Keys)}."
                                : $"Disabling cache in repository writer for entities {string.Join(", ", entitiesInProgress.Keys)}.");
                    }
                });

                if (failedWriters.Count > 0)
                {
                    var message = $"Failed to disable cache for some repository writers: {string.Join(", ", failedWriters.Keys)}.";
                    UpdateStatusMessage(message);
                    this.logger.Fatal(message);
                    throw new OperationCanceledException(message);
                }

                UpdateStatusMessage("Cache in repository writers disabled.");
            }
        }

        private void RepublishAllEvents(IEnumerable<CommittedEvent> eventStream, int allEventsCount, int skipEventsCount = 0,
            IEventHandler[] handlers = null)
        {
            this.totalEventsToRebuildCount = allEventsCount;
            this.skippedEventsCount = skipEventsCount;
            this.processedEventsCount = this.skippedEventsCount;
            this.maxAllowedFailedEvents = Math.Max(100, (int) (allEventsCount*0.01));

            ThrowIfShouldStopViewsRebuilding();

            this.logger.Info("Starting rebuild Read Side. Skip Events Count: " + skipEventsCount);

            UpdateStatusMessage("Determining count of events to be republished.");

            this.lastRebuildDate = DateTime.Now;

            UpdateStatusMessage("Acquiring first portion of events.");

            DateTime republishStarted = DateTime.Now;
            UpdateStatusMessage(
                "Acquiring first portion of events."
                + Environment.NewLine
                + GetReadablePublishingDetails(republishStarted, processedEventsCount, allEventsCount, this.FailedEventsCount, skipEventsCount));

            handlersWithStopwatches = handlers.ToDictionary(x => x, x => new Stopwatch());

            foreach (CommittedEvent @event in eventStream)
            {
                ThrowIfShouldStopViewsRebuilding();

                string eventTypeName = @event.Payload.GetType().Name;
                UpdateStatusMessage($"Publishing event {this.processedEventsCount + 1} {eventTypeName}. ");

                EventHandlerExceptionDelegate eventHandlerExceptionDelegate = (nonCriticalEventHandlerException) =>
                {
                    string message =
                        $"Failed to publish event {this.processedEventsCount + 1} of {this.totalEventsToRebuildCount} - {eventTypeName} ({@event.EventIdentifier.FormatGuid()})";
                    this.SaveErrorForStatusReport(
                        $"{nonCriticalEventHandlerException.EventHandlerType.Name}.{nonCriticalEventHandlerException.EventType.Name}: {message}",
                        nonCriticalEventHandlerException);
                };

                try
                {
                    this.eventBus.OnCatchingNonCriticalEventHandlerException += eventHandlerExceptionDelegate;

                    using (GlobalStopwatcher.Scope("Publish event", eventTypeName))
                    {
                        this.eventBus.PublishEventToHandlers(@event, handlersWithStopwatches);
                    }
                }
                catch (Exception exception)
                {
                    string message =
                        $"Failed to publish event {this.processedEventsCount + 1} of {this.totalEventsToRebuildCount} - {eventTypeName} ({@event.EventIdentifier.FormatGuid()})";
                    this.SaveErrorForStatusReport(message, exception);
                    this.logger.Error(message, exception);

                }
                finally
                {
                    this.eventBus.OnCatchingNonCriticalEventHandlerException -= eventHandlerExceptionDelegate;
                }

                this.processedEventsCount++;

                if (this.FailedEventsCount >= maxAllowedFailedEvents)
                {
                    var message = $"Failed to rebuild read side. Too many events failed: {this.FailedEventsCount}. Last processed event count: {this.processedEventsCount}";
                    UpdateStatusMessage(message);
                    this.logger.Fatal(message);
                    throw new OperationCanceledException(message);
                }

                UpdateStatusMessage($"Done publishing event {this.processedEventsCount}, {eventTypeName}. EventSourceId: {@event.EventSourceId:N}. Waiting for next event or stream end...");
            }

            this.logger.Info($"Rebuild of read side finished successfully. Processed {this.processedEventsCount} events, failed {this.FailedEventsCount}");
        }

        private static string GetReadablePublishingDetails(DateTime republishStarted,
           int processedEventsCount, int allEventsCount, int failedEventsCount, int skippedEventsCount)
        {
            int republishedEventsCount = processedEventsCount - skippedEventsCount;

            TimeSpan republishTimeSpent = DateTime.Now - republishStarted;

            int speedInEventsPerMinute = (int)(
                republishTimeSpent.TotalSeconds == 0
                ? 0
                : 60 * republishedEventsCount / republishTimeSpent.TotalSeconds);

            TimeSpan estimatedTotalRepublishTime = TimeSpan.FromMilliseconds(
                republishedEventsCount == 0
                ? 0
                : republishTimeSpent.TotalMilliseconds / republishedEventsCount * allEventsCount);

            return string.Format(
                "Processed events: {1}. Total events: {2}. Skipped events: {3} Failed events: {4}.{0}Time spent republishing: {5}. Speed: {6} events per minute. Estimated time: {7}.",
                Environment.NewLine,
                processedEventsCount, allEventsCount, skippedEventsCount, failedEventsCount,
                republishTimeSpent.ToString(@"hh\:mm\:ss"), speedInEventsPerMinute, estimatedTotalRepublishTime.ToString(@"hh\:mm\:ss"));
        }


        private static void ThrowIfShouldStopViewsRebuilding()
        {
            if (shouldStopViewsRebuilding)
            {
                shouldStopViewsRebuilding = false;

                const string stopRequestMessage = "Read side rebuild stopped by request.";

                UpdateStatusMessage(stopRequestMessage);
                throw new OperationCanceledException(stopRequestMessage);
            }
        }

        private static void UpdateStatusMessage(string newMessage)
        {
            statusMessage = $"{DateTime.Now}: {newMessage}";
        }

        private static string GetStorageEntityName(IReadSideStorage writer)
        {
            return writer.ViewType.Name;
        }

        #region Error reporting methods

        private void SaveErrorForStatusReport(string message, Exception exception)
        {
            lock (ErrorsLockObject)
            {
                errors.Add(Tuple.Create(DateTime.Now, message, exception));
            }

            this.logger.Error(message, exception);
        }

        private static IEnumerable<T> ReverseList<T>(List<T> list)
        {
            for (int indexOfElement = list.Count - 1; indexOfElement >= 0; indexOfElement--)
                yield return list[indexOfElement];
        }

        private static string GetFullUnwrappedExceptionText(Exception exception)
        {
            return string.Join(Environment.NewLine, exception.UnwrapAllInnerExceptions());
        }

        #endregion // Error reporting methods
    }
}