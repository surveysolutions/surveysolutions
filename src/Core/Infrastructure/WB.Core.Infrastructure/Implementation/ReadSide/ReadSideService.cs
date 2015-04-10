using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Services;
using WB.Core.Infrastructure.Transactions;

namespace WB.Core.Infrastructure.Implementation.ReadSide
{
    public class ReadSideService : IReadSideAdministrationService
    {
        internal static int InstanceCount = 0;

        private int totalEventsToRebuildCount = 0;

        private int FailedEventsCount
        {
            get { return errors.Count; }
        }

        private int processedEventsCount = 0;
        private int skippedEventsCount = 0;
        private DateTime lastRebuildDate = DateTime.Now;

        private const int MaxAllowedFailedEvents = 100;

        private static readonly object RebuildAllViewsLockObject = new object();
        private static readonly object ErrorsLockObject = new object();

        private static bool areViewsBeingRebuiltNow = false;
        private static bool shouldStopViewsRebuilding = false;

        private static string statusMessage;
        private static List<Tuple<DateTime, string, Exception>> errors = new List<Tuple<DateTime, string, Exception>>();

        private readonly IStreamableEventStore eventStore;
        private readonly IEventDispatcher eventBus;
        private readonly ILogger logger;
        private readonly IReadSideCleaner readSideCleaner;
        private Dictionary<IEventHandler, Stopwatch> handlersWithStopwatches;
        private readonly ITransactionManagerProviderManager transactionManagerProviderManager;

        static ReadSideService()
        {
            UpdateStatusMessage("No administration operations were performed so far.");
        }

        public ReadSideService(IStreamableEventStore eventStore, 
            IEventDispatcher eventBus, 
            ILogger logger,
            IReadSideCleaner readSideCleaner, 
            ITransactionManagerProviderManager transactionManagerProviderManager)
        {
            if (InstanceCount > 0)
                throw new Exception(string.Format("Trying to create a new instance of RavenReadSideService when following count of instances exists: {0}.", InstanceCount));

            Interlocked.Increment(ref InstanceCount);

            this.eventStore = eventStore;
            this.eventBus = eventBus;
            this.logger = logger;
            this.readSideCleaner = readSideCleaner;
            this.transactionManagerProviderManager = transactionManagerProviderManager;
        }

        #region IReadLayerStatusService implementation

        public bool AreViewsBeingRebuiltNow()
        {
            return areViewsBeingRebuiltNow;
        }

        #endregion // IReadLayerStatusService implementation

        #region IReadLayerAdministrationService implementation

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

            TimeSpan republishTimeSpent = this.AreViewsBeingRebuiltNow() ? DateTime.Now - this.lastRebuildDate : TimeSpan.Zero;

            int speedInEventsPerMinute = (int)(
                republishTimeSpent.TotalSeconds == 0
                ? 0
                : 60 * republishedEventsCount / republishTimeSpent.TotalSeconds);

            TimeSpan estimatedTotalRepublishTime = TimeSpan.FromMilliseconds(
                republishedEventsCount == 0
                ? 0
                : republishTimeSpent.TotalMilliseconds / republishedEventsCount * this.totalEventsToRebuildCount);


            return new ReadSideStatus()
            {
                IsRebuildRunning = this.AreViewsBeingRebuiltNow(),
                CurrentRebuildStatus = statusMessage,
                LastRebuildDate = this.lastRebuildDate,
                EventPublishingDetails = new ReadSideEventPublishingDetails()
                {
                    ProcessedEvents = republishedEventsCount,
                    EstimatedTime = estimatedTotalRepublishTime,
                    FailedEvents = this.FailedEventsCount,
                    SkippedEvents = this.skippedEventsCount,
                    Speed = speedInEventsPerMinute,
                    TimeSpent = republishTimeSpent,
                    TotalEvents = this.totalEventsToRebuildCount

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
                RebuildErrors = ReverseList(errors)
                    .Select(error => new ReadSideRepositoryWriterError()
                    {
                        ErrorTime = error.Item1,
                        ErrorMessage = error.Item2,
                        InnerException = GetFullUnwrappedExceptionText(error.Item3)
                    }),
                ReadSideDenormalizerStatistics=GetRebuildDenormalizerStatistics()
            };
        }

        private string CreateViewName(object storage)
        {
            var readSideStorage = storage as IReadSideStorage;

            if (readSideStorage != null)
                return GetStorageEntityName(readSideStorage);

            return storage.GetType().Name;
        }

        #endregion // IReadLayerAdministrationService implementation

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
                    "Not all handlers supports partial rebuild. Handlers which are not supporting partial rebuild are {0}" +
                    string.Join((string) ",", (IEnumerable<string>) handlers.Where(h => !atomicEventHandlers.Contains(h)).Select(h => h.Name));

                UpdateStatusMessage(message);

                throw new Exception(message);
            }

            try
            {
                foreach (var atomicEventHandler in atomicEventHandlers)
                {
                    ThrowIfShouldStopViewsRebuilding();

                    var cleanerName = atomicEventHandler.Name;

                    foreach (var eventSourceId in eventSourceIds)
                    {
                        UpdateStatusMessage(string.Format("Cleaning views for {0} and event source {1}", cleanerName, eventSourceId));
                        atomicEventHandler.CleanWritersByEventSource(eventSourceId);
                        UpdateStatusMessage(string.Format("Views for {0}  and event source {1} was cleaned.", cleanerName, eventSourceId));
                    }
                }

                try
                {
                    this.EnableWritersCacheForHandlers(handlers);
                    this.transactionManagerProviderManager.PinRebuildReadSideTransactionManager();

                    foreach (var eventSourceId in eventSourceIds)
                    {
                        var eventsToPublish = this.eventStore.ReadFrom(eventSourceId, 0, long.MaxValue);
                        this.RepublishAllEvents(eventsToPublish, eventsToPublish.Count(), handlers: handlers);
                    }
                }
                finally
                {
                    this.transactionManagerProviderManager.UnpinTransactionManager();
                    this.DisableWritersCacheForHandlers(handlers);
                }

                UpdateStatusMessage("Rebuild specific views succeeded.");
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

        private void RebuildViewsImpl(int skipEvents, IEventHandler[] handlers, bool isPartiallyRebuild)
        {
            try
            {
                areViewsBeingRebuiltNow = true;

                errors.Clear();

                try
                {
                    this.transactionManagerProviderManager.PinRebuildReadSideTransactionManager();

                    if (skipEvents == 0)
                    {
                        this.CleanUpWritersForHandlers(handlers, isPartiallyRebuild);
                    }

                    try
                    {
                        this.EnableWritersCacheForHandlers(handlers);

                        this.RepublishAllEvents(this.GetEventStream(skipEvents), this.eventStore.CountOfAllEvents(), skipEventsCount: skipEvents, handlers: handlers);
                    }
                    finally
                    {
                        this.DisableWritersCacheForHandlers(handlers);

                        if(!isPartiallyRebuild && this.readSideCleaner != null) 
                            this.readSideCleaner.CreateIndexesAfterRebuildReadSide();
                    }
                }
                finally
                {
                    this.transactionManagerProviderManager.UnpinTransactionManager();
                }

                UpdateStatusMessage("Rebuild specific views succeeded.");
                logger.Info("Rebuild views succeeded");
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

        private IEnumerable<CommittedEvent> GetEventStream(int skipEventsCount)
        {
            if (skipEventsCount > 0)
            {
                UpdateStatusMessage(string.Format("Skipping {0} events.", skipEventsCount));
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


        private void CleanUpWritersForHandlers(IEnumerable<IEventHandler> handlers, bool isPartiallyRebuild)
        {
            var cleaners = handlers.SelectMany(x=>x.Writers.OfType<IReadSideRepositoryCleaner>())
                  .Distinct()
                  .ToArray();

            if(!isPartiallyRebuild && this.readSideCleaner != null) this.readSideCleaner.ReCreateViewDatabase();

            foreach (var readSideRepositoryCleaner in cleaners)
            {
                ThrowIfShouldStopViewsRebuilding();

                var cleanerName = this.CreateViewName(readSideRepositoryCleaner);

                UpdateStatusMessage(string.Format("Deleting views for {0}", cleanerName));

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

                UpdateStatusMessage(string.Format("Views for {0} was deleted.", cleanerName));
            }
        }

        private void EnableWritersCacheForHandlers(IEnumerable<IEventHandler> handlers)
        {
            UpdateStatusMessage("Enabling cache in repository writers.");

            var writers = handlers.SelectMany(x => x.Writers.OfType<IChacheableRepositoryWriter>())
               .Distinct()
               .ToArray();

            foreach (IChacheableRepositoryWriter writer in writers)
            {
                writer.EnableCache();
            }

            UpdateStatusMessage("Cache in repository writers enabled.");
        }

        private void DisableWritersCacheForHandlers(IEnumerable<IEventHandler> handlers)
        {
            UpdateStatusMessage("Disabling cache in repository writers.");

            var writers = handlers.SelectMany(x => x.Writers.OfType<IChacheableRepositoryWriter>())
             .Distinct()
             .ToArray();

            foreach (IChacheableRepositoryWriter writer in writers)
            {
                UpdateStatusMessage(string.Format(
                    "Disabling cache in repository writer for entity {0}.",
                    GetStorageEntityName(writer)));

                try
                {
                    this.transactionManagerProviderManager.GetTransactionManager().BeginCommandTransaction();
                    writer.DisableCache();
                    this.transactionManagerProviderManager.GetTransactionManager().CommitCommandTransaction();
                }
                catch (Exception exception)
                {
                    this.transactionManagerProviderManager.GetTransactionManager().RollbackCommandTransaction();
                    string message = string.Format("Failed to disable cache and store data to repository for writer {0}.", GetStorageEntityName(writer));
                    this.SaveErrorForStatusReport(message, exception);
                    UpdateStatusMessage(message);
                }
            }

            UpdateStatusMessage("Cache in repository writers disabled.");
        }

        private void RepublishAllEvents(IEnumerable<CommittedEvent> eventStream, int allEventsCount, int skipEventsCount = 0,
            IEventHandler[] handlers = null)
        {
            this.totalEventsToRebuildCount = allEventsCount;
            this.skippedEventsCount = skipEventsCount;
            this.processedEventsCount = this.skippedEventsCount;

            ThrowIfShouldStopViewsRebuilding();

            this.logger.Info("Starting rebuild Read Side");

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
                UpdateStatusMessage(string.Format("Publishing event {0} {1}. ", this.processedEventsCount + 1, eventTypeName));

                try
                {
                    this.eventBus.PublishEventToHandlers(@event, handlersWithStopwatches);
                }
                catch (Exception exception)
                {
                    string message = string.Format("Failed to publish event {0} of {1} ({2})", this.processedEventsCount + 1, this.totalEventsToRebuildCount, @event.EventIdentifier);
                    this.SaveErrorForStatusReport(message, exception);
                    this.logger.Error(message, exception);

                }

                this.processedEventsCount++;

                if (this.FailedEventsCount >= MaxAllowedFailedEvents)
                {
                    var message = string.Format("Failed to rebuild read side. Too many events failed: {0}. Last processed event count: {1}", this.FailedEventsCount, this.processedEventsCount);
                    UpdateStatusMessage(message);
                    this.logger.Error(message);
                    throw new OperationCanceledException(message);
                }

                UpdateStatusMessage(string.Format("Done publishing event {0}, {1}. EventSourceId: {2:N}. Waiting for next event or stream end...", 
                    this.processedEventsCount, 
                    eventTypeName, 
                    @event.EventSourceId));
            }

            this.logger.Info(String.Format("Rebuild of read side finished sucessfuly. Processed {0} events, failed {1}", this.processedEventsCount, this.FailedEventsCount));
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

                const string stopRequestMessage = "Views rebuilding stopped by request.";

                UpdateStatusMessage(stopRequestMessage);
                throw new OperationCanceledException(stopRequestMessage);
            }
        }

        private static void UpdateStatusMessage(string newMessage)
        {
            statusMessage = string.Format("{0}: {1}", DateTime.Now, newMessage);
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