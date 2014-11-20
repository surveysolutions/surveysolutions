using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide
{
    internal class RavenReadSideService : IReadSideStatusService, IReadSideAdministrationService
    {
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

        static RavenReadSideService()
        {
            UpdateStatusMessage("No administration operations were performed so far.");
        }

        public RavenReadSideService(IStreamableEventStore eventStore, IEventDispatcher eventBus, ILogger logger)
        {
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

        public string GetReadableStatus()
        {
            return string.Format("{1}{0}{0}Are views being rebuilt now: {2}{0}{0}{3}{0}{0}{4}",
                Environment.NewLine,
                statusMessage,
                areViewsBeingRebuiltNow ? "Yes" : "No",
                this.GetReadableListOfRepositoryWriters(),
                GetReadableErrors());
        }

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

        public IEnumerable<EventHandlerDescription> GetAllAvailableHandlers()
        {
            return
                this.eventBus.GetAllRegistredEventHandlers()
                    .Select(
                        h =>
                        new EventHandlerDescription(h.Name, h.Readers.Select(CreateViewName).ToArray(),
                                                    h.Writers.Select(CreateViewName).ToArray(), h is IAtomicEventHandler))
                    .ToList();
        }

        private string CreateViewName(object storage)
        {
            var readSideRepositoryWriter = storage as IReadSideRepositoryWriter;
            if (readSideRepositoryWriter != null)
                return GetRepositoryEntityName(readSideRepositoryWriter);

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
                        this.RebuildViewsImpl(skipEvents, eventBus.GetAllRegistredEventHandlers());
                    }
                }
            }
        }

        private IEventHandler[] GetListOfEventHandlersForRebuild(string[] handlerNames)
        {
            var allHandlers = this.eventBus.GetAllRegistredEventHandlers();
            var result = new List<IEventHandler>();
            foreach (var eventHandler in allHandlers)
            {
                if (handlerNames.Contains(eventHandler.Name))
                    result.Add(eventHandler);
            }
            return result.ToArray();
        }

        private void RebuildViewsByEventSourcesImpl(Guid[] eventSourceIds, IEventHandler[] handlers)
        {
            areViewsBeingRebuiltNow = true;

            var atomicEventHandlers = handlers.OfType<IAtomicEventHandler>().ToArray();

            if (atomicEventHandlers.Length != handlers.Length)
                throw new Exception(
                    "Not all handlers supports partial rebuild. Handlers which are not supporting partial rebuild are {0}" +
                        string.Join(",", handlers.Where(h => !atomicEventHandlers.Contains(h)).Select(h => h.Name)));
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

                string republishDetails = "<<NO DETAILS>>";

                try
                {

                    this.EnableWritersCacheForHandlers(handlers);

                    foreach (var eventSourceId in eventSourceIds)
                    {
                        var eventsToPublish = eventStore.ReadFrom(eventSourceId, 0, long.MaxValue);
                        republishDetails += Environment.NewLine +
                            this.RepublishAllEvents(eventsToPublish, eventsToPublish.Count(),
                                handlers: handlers);
                    }
                }
                finally
                {
                    this.DisableWritersCacheForHandlers(handlers);

                    UpdateStatusMessage("Rebuild specific views succeeded." + Environment.NewLine + republishDetails);

                }
            }
            catch (Exception exception)
            {
                this.SaveErrorForStatusReport("Unexpected error occurred", exception);
                UpdateStatusMessage(string.Format("Unexpectedly failed. Last status message:{0}{1}",
                    Environment.NewLine, statusMessage));
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

                if (skipEvents == 0)
                {
                    this.CleanUpWritersForHandlers(handlers);
                }

                string republishDetails = "<<NO DETAILS>>";

                try
                {
                    this.EnableWritersCacheForHandlers(handlers);

                    republishDetails = this.RepublishAllEvents(GetEventStream(skipEvents), this.eventStore.CountOfAllEvents(),skipEventsCount: skipEvents, handlers: handlers);
                }
                finally
                {
                    this.DisableWritersCacheForHandlers(handlers);

                    UpdateStatusMessage("Rebuild specific views succeeded." + Environment.NewLine + republishDetails);
                }
            }
            catch (Exception exception)
            {
                this.SaveErrorForStatusReport("Unexpected error occurred", exception);
                UpdateStatusMessage(string.Format("Unexpectedly failed. Last status message:{0}{1}",
                    Environment.NewLine, statusMessage));
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

                var cleanerName = CreateViewName(readSideRepositoryCleaner);
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
                    GetRepositoryEntityName(writer)));

                try
                {
                    writer.DisableCache();
                }
                catch (Exception exception)
                {
                    this.SaveErrorForStatusReport(
                        string.Format("Failed to disable cache and store data to repository for writer {0}.",
                            writer.GetType()),
                        exception);
                }
            }

            UpdateStatusMessage("Cache in repository writers disabled.");
        }

        private string RepublishAllEvents(IEnumerable<CommittedEvent> eventStream, int allEventsCount, int skipEventsCount = 0,
            IEnumerable<IEventHandler> handlers = null)
        {
            int processedEventsCount = skipEventsCount;
            int failedEventsCount = 0;

            ThrowIfShouldStopViewsRebuilding();

            this.logger.Info("Starting rebuild Read Layer");

            UpdateStatusMessage("Determining count of events to be republished.");

            DateTime republishStarted = DateTime.Now;
            UpdateStatusMessage(
                "Acquiring first portion of events. "
                    +
                    GetReadablePublishingDetails(republishStarted, processedEventsCount, allEventsCount, failedEventsCount, skipEventsCount));


            foreach (CommittedEvent @event in eventStream)
            {
                ThrowIfShouldStopViewsRebuilding(GetReadablePublishingDetails(republishStarted, processedEventsCount, allEventsCount,
                    failedEventsCount, skipEventsCount));

                UpdateStatusMessage(
                    string.Format("Publishing event {0}. ", processedEventsCount + 1)
                        +
                        GetReadablePublishingDetails(republishStarted, processedEventsCount, allEventsCount, failedEventsCount,
                            skipEventsCount));

                try
                {
                    this.eventBus.PublishEventToHandlers(@event, handlers);
                }
                catch (Exception exception)
                {
                    this.SaveErrorForStatusReport(
                        string.Format("Failed to publish event {0} of {1} ({2})",
                            processedEventsCount + 1, allEventsCount, @event.EventIdentifier),
                        exception);

                    failedEventsCount++;
                }

                processedEventsCount++;

                if (failedEventsCount >= MaxAllowedFailedEvents)
                    throw new Exception(string.Format("Failed to rebuild read layer. Too many events failed: {0}.", failedEventsCount));
            }

            UpdateStatusMessage(
                "Acquiring next portion of events. "
                    +
                    GetReadablePublishingDetails(republishStarted, processedEventsCount, allEventsCount, failedEventsCount, skipEventsCount));

            this.logger.Info(String.Format("Processed {0} events, failed {1}", processedEventsCount, failedEventsCount));

            UpdateStatusMessage(string.Format("All events were republished. "
                + GetReadablePublishingDetails(republishStarted, processedEventsCount, allEventsCount, failedEventsCount, skipEventsCount)));

            return GetReadablePublishingDetails(republishStarted, processedEventsCount, allEventsCount, failedEventsCount, skipEventsCount);
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

        private static void ThrowIfShouldStopViewsRebuilding(string readableStatus = "")
        {
            if (shouldStopViewsRebuilding)
            {
                shouldStopViewsRebuilding = false;
                throw new Exception("Views rebuilding stopped by request. " + readableStatus);
            }
        }

        private static void UpdateStatusMessage(string newMessage)
        {
            statusMessage = string.Format("{0}: {1}", DateTime.Now, newMessage);
        }

        private string GetReadableListOfRepositoryWriters()
        {
            List<IReadSideRepositoryWriter> writers =eventBus.GetAllRegistredEventHandlers().SelectMany(x=>x.Writers.OfType<IReadSideRepositoryWriter>()).Distinct().ToList();

            bool areThereNoWriters = writers.Count == 0;
#warning to Tolik: calls to dictionary (writer cache) from other thread rais exceptions because Dictionary is not thread safe
            return areThereNoWriters
                ? "Registered writers: None"
                : string.Format(
                    "Registered writers: {1}{0}{2}",
                    Environment.NewLine,
                    writers.Count,
                    string.Join(
                        Environment.NewLine,
                        writers
                            .Select(writer => string.Format("{0,-40} ({1})", GetRepositoryEntityName(writer), writer.GetReadableStatus()))
                            .OrderBy(_ => _)
                            .ToArray()));
        }

        private string GetRepositoryEntityName(IReadSideRepositoryWriter writer)
        {
            var arguments = writer.ViewType.GetGenericArguments();
            if (!arguments.Any())
                return writer.ViewType.Name;
            return arguments.Single().Name;
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

        private static string GetReadableErrors()
        {
            lock (ErrorsLockObject)
            {
                bool areThereNoErrors = errors.Count == 0;

                return areThereNoErrors
                    ? "Errors: None"
                    : string.Format(
                        "Errors: {1}{0}{0}{2}",
                        Environment.NewLine,
                        errors.Count,
                        string.Join(
                            Environment.NewLine + Environment.NewLine,
                            ReverseList(errors)
                                .Select((error, index) => GetReadableError(error, shouldShowStackTrace: index < 10))
                                .ToArray()));
            }
        }

        private static IEnumerable<T> ReverseList<T>(List<T> list)
        {
            for (int indexOfElement = list.Count - 1; indexOfElement >= 0; indexOfElement--)
                yield return list[indexOfElement];
        }

        private static string GetReadableError(Tuple<DateTime, string, Exception> error, bool shouldShowStackTrace)
        {
            return string.Format("{1}: {2}{0}{3}", Environment.NewLine,
                error.Item1,
                error.Item2,
                shouldShowStackTrace ? GetFullUnwrappedExceptionText(error.Item3) : error.Item3.Message);
        }

        private static string GetFullUnwrappedExceptionText(Exception exception)
        {
            return string.Join(Environment.NewLine, exception.UnwrapAllInnerExceptions());
        }

        #endregion // Error reporting methods
    }
}