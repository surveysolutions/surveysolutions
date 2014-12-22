using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.Infrastructure.Implementation.ReadSide
{
    public class ReadSideService : IReadSideAdministrationService
    {
        internal static int InstanceCount = 0;

        private int totalEventsToRebuildCount = 0;
        private int failedEventsCount = 0;

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

        static ReadSideService()
        {
            UpdateStatusMessage("No administration operations were performed so far.");
        }

        public ReadSideService(IStreamableEventStore eventStore, IEventDispatcher eventBus, ILogger logger)
        {
            if (InstanceCount > 0)
                throw new Exception(string.Format("Trying to create a new instance of RavenReadSideService when following count of instances exists: {0}.", InstanceCount));

            Interlocked.Increment(ref InstanceCount);

            this.eventStore = eventStore;
            this.eventBus = eventBus;
            this.logger = logger;
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
                    FailedEvents = this.failedEventsCount,
                    SkippedEvents = this.skippedEventsCount,
                    Speed = speedInEventsPerMinute,
                    TimeSpent = republishTimeSpent,
                    TotalEvents = this.totalEventsToRebuildCount

                },
                StatusByRepositoryWriters = this.eventBus.GetAllRegistredEventHandlers()
                    .SelectMany(x => x.Writers.OfType<IReadSideRepositoryWriter>())
                    .Distinct()
                    .Select(
                        writer =>
                            new ReadSideRepositoryWriterStatus()
                            {
                                WriterName = this.GetRepositoryEntityName(writer),
                                Status = writer.GetReadableStatus()
                            }),
                RebuildErrors = ReverseList(errors)
                    .Select(error => new ReadSideRepositoryWriterError()
                    {
                        ErrorTime = error.Item1,
                        ErrorMessage = error.Item2,
                        InnerException = GetFullUnwrappedExceptionText(error.Item3)
                    })
            };
        }

        private string CreateViewName(object storage)
        {
            var readSideRepositoryWriter = storage as IReadSideRepositoryWriter;
            if (readSideRepositoryWriter != null)
                return this.GetRepositoryEntityName(readSideRepositoryWriter);

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
                        this.RebuildViewsImpl(skipEvents, handlers);
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
                        this.RebuildViewsImpl(skipEvents, this.eventBus.GetAllRegistredEventHandlers());
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

                    foreach (var eventSourceId in eventSourceIds)
                    {
                        var eventsToPublish = this.eventStore.ReadFrom(eventSourceId, 0, long.MaxValue);
                        this.RepublishAllEvents(eventsToPublish, eventsToPublish.Count(), handlers: handlers);
                    }
                }
                finally
                {
                    this.DisableWritersCacheForHandlers(handlers);

                    UpdateStatusMessage("Rebuild specific views succeeded.");

                }
            }
            catch (Exception exception)
            {
                this.SaveErrorForStatusReport("Unexpected error occurred", exception);
                throw;
            }
            finally
            {
                areViewsBeingRebuiltNow = false;
            }
        }

        private void RebuildViewsImpl(int skipEvents, IEnumerable<IEventHandler> handlers)
        {
            try
            {
                areViewsBeingRebuiltNow = true;

                errors.Clear();

                if (skipEvents == 0)
                {
                    this.CleanUpWritersForHandlers(handlers);
                }

                try
                {
                    this.EnableWritersCacheForHandlers(handlers);
                    this.RepublishAllEvents(this.GetEventStream(skipEvents), this.eventStore.CountOfAllEvents(),skipEventsCount: skipEvents, handlers: handlers);
                }
                finally
                {
                    this.DisableWritersCacheForHandlers(handlers);

                    UpdateStatusMessage("Rebuild specific views succeeded.");
                }
            }
            catch (Exception exception)
            {
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
            foreach (CommittedEvent[] eventBulk in this.eventStore.GetAllEvents(skipEvents: skipEventsCount))
            {
                foreach (var committedEvent in eventBulk)
                {
                    yield return committedEvent;
                }
            }
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
                UpdateStatusMessage(string.Format("Deleting views for {0}", cleanerName));
                readSideRepositoryCleaner.Clear();
                UpdateStatusMessage(string.Format("Views for {0} was deleted.", cleanerName));
            }
        }

        private void EnableWritersCacheForHandlers(IEnumerable<IEventHandler> handlers)
        {
            UpdateStatusMessage("Enabling cache in repository writers.");

            var writers = handlers.SelectMany(x => x.Writers.OfType<IReadSideRepositoryWriter>())
               .Distinct()
               .ToArray();

            foreach (IReadSideRepositoryWriter writer in writers)
            {
                writer.EnableCache();
            }

            UpdateStatusMessage("Cache in repository writers enabled.");
        }

        private void DisableWritersCacheForHandlers(IEnumerable<IEventHandler> handlers)
        {
            UpdateStatusMessage("Disabling cache in repository writers.");

            var writers = handlers.SelectMany(x => x.Writers.OfType<IReadSideRepositoryWriter>())
             .Distinct()
             .ToArray();

            foreach (IReadSideRepositoryWriter writer in writers)
            {
                UpdateStatusMessage(string.Format(
                    "Disabling cache in repository writer for entity {0}.",
                    this.GetRepositoryEntityName(writer)));

                try
                {
                    writer.DisableCache();
                }
                catch (Exception exception)
                {
                    this.SaveErrorForStatusReport(
                        string.Format("Failed to disable cache and store data to repository for writer {0}.",
                            this.GetRepositoryEntityName(writer)),
                        exception);
                }
            }

            UpdateStatusMessage("Cache in repository writers disabled.");
        }

        private void RepublishAllEvents(IEnumerable<CommittedEvent> eventStream, int allEventsCount, int skipEventsCount = 0,
            IEnumerable<IEventHandler> handlers = null)
        {
            this.totalEventsToRebuildCount = allEventsCount;
            this.skippedEventsCount = skipEventsCount;
            this.processedEventsCount = this.skippedEventsCount;

            ThrowIfShouldStopViewsRebuilding();

            this.logger.Info("Starting rebuild Read Layer");

            UpdateStatusMessage("Determining count of events to be republished.");

            this.lastRebuildDate = DateTime.Now;
            UpdateStatusMessage("Acquiring first portion of events.");

            DateTime republishStarted = DateTime.Now;
            UpdateStatusMessage(
                "Acquiring first portion of events."
                + Environment.NewLine
                + GetReadablePublishingDetails(republishStarted, processedEventsCount, allEventsCount, failedEventsCount, skipEventsCount));

            foreach (CommittedEvent @event in eventStream)
            {
                ThrowIfShouldStopViewsRebuilding();

                UpdateStatusMessage(string.Format("Publishing event {0}. ", this.processedEventsCount + 1));


                try
                {
                    this.eventBus.PublishEventToHandlers(@event, handlers);
                }
                catch (Exception exception)
                {
                    this.SaveErrorForStatusReport(
                        string.Format("Failed to publish event {0} of {1} ({2})",
                            this.processedEventsCount + 1, this.totalEventsToRebuildCount, @event.EventIdentifier),
                        exception);

                    this.failedEventsCount++;
                }

                this.processedEventsCount++;

                if (this.failedEventsCount >= MaxAllowedFailedEvents)
                {
                    var message = string.Format("Failed to rebuild read layer. Too many events failed: {0}.",
                        this.failedEventsCount);
                    UpdateStatusMessage(message);
                    throw new Exception(message);
                }
            }

            UpdateStatusMessage("Acquiring next portion of events.");

            this.logger.Info(String.Format("Processed {0} events, failed {1}", this.processedEventsCount, this.failedEventsCount));

            UpdateStatusMessage("All events were republished.");

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
                throw new Exception(stopRequestMessage);
            }
        }

        private static void UpdateStatusMessage(string newMessage)
        {
            statusMessage = string.Format("{0}: {1}", DateTime.Now, newMessage);
        }

        private string GetRepositoryEntityName(IReadSideRepositoryWriter writer)
        {
           /* var arguments = writer.ViewType.GetGenericArguments();
            if (!arguments.Any())*/
                return writer.ViewType.Name;
           // return arguments.Single().Name;
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