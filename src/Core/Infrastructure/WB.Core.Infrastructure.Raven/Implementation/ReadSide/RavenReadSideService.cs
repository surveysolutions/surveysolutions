using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;

using Raven.Abstractions.Data;
using Raven.Abstractions.Indexing;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Extensions;

using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernel.Logger;

namespace WB.Core.Infrastructure.Raven.Implementation.ReadSide
{
    internal class RavenReadSideService : IReadSideStatusService, IReadSideAdministrationService
    {
        private const int MaxAllowedFailedEvents = 100;

        private static readonly object RebuildAllViewsLockObject = new object();
        private static readonly object ErrorsLockObject = new object();

        private static bool areViewsBeingRebuiltNow = false;
        private static bool shouldStopViewsRebuilding = false;

        private static string statusMessage;
        private static List<Tuple<DateTime, string, Exception>> errors = new List<Tuple<DateTime,string,Exception>>();

        private readonly IStreamableEventStore eventStore;
        private readonly IEventBus eventBus;
        private readonly DocumentStore ravenStore;
        private readonly ILog logger;
        private readonly IRavenReadSideRepositoryWriterRegistry writerRegistry;

        static RavenReadSideService()
        {
            UpdateStatusMessage("No administration operations were performed so far.");
        }

        public RavenReadSideService(IStreamableEventStore eventStore, IEventBus eventBus, DocumentStore ravenStore, ILog logger, IRavenReadSideRepositoryWriterRegistry writerRegistry)
        {
            this.eventStore = eventStore;
            this.eventBus = eventBus;
            this.ravenStore = ravenStore;
            this.logger = logger;
            this.writerRegistry = writerRegistry;
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
                this.GetReadableListOfWriters(),
                GetReadableErrors());
        }

        public void RebuildAllViewsAsync()
        {
            new Task(this.RebuildAllViews).Start();
        }

        public void StopAllViewsRebuilding()
        {
            if (!areViewsBeingRebuiltNow)
                return;

            shouldStopViewsRebuilding = true;
        }

        #endregion // IReadLayerAdministrationService implementation

        private void RebuildAllViews()
        {
            if (!areViewsBeingRebuiltNow)
            {
                lock (RebuildAllViewsLockObject)
                {
                    if (!areViewsBeingRebuiltNow)
                    {
                        this.RebuildAllViewsImpl();
                    }
                }
            }
        }

        private void RebuildAllViewsImpl()
        {
            try
            {
                areViewsBeingRebuiltNow = true;

                this.DeleteAllViews();

                this.RepublishAllEvents();
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

        private void DeleteAllViews()
        {
            ThrowIfShouldStopViewsRebuilding();

            UpdateStatusMessage("Determining count of views to be deleted.");

            this.ravenStore
                .DatabaseCommands
                .EnsureDatabaseExists("Views");

            this.ravenStore
                .DatabaseCommands
                .ForDatabase("Views")
                .PutIndex(
                    "AllViews",
                    new IndexDefinition { Map = "from doc in docs let DocId = doc[\"@metadata\"][\"@id\"] select new {DocId};" },
                    overwrite: true);

            int initialViewCount;
            using (IDocumentSession session = this.ravenStore.OpenSession("Views"))
            {
                // this will also materialize index if it is out of date or was just created
                initialViewCount = session
                    .Query<object>("AllViews")
                    .Customize(customization => customization.WaitForNonStaleResultsAsOfNow())
                    .Count();
            }

            ThrowIfShouldStopViewsRebuilding();

            UpdateStatusMessage(string.Format("Deleting {0} views.", initialViewCount));

            this.ravenStore
                .DatabaseCommands
                .ForDatabase("Views")
                .DeleteByIndex("AllViews", new IndexQuery());

            UpdateStatusMessage("Checking remaining views count.");

            int resultViewCount;
            using (IDocumentSession session = this.ravenStore.OpenSession("Views"))
            {
                resultViewCount = session
                    .Query<object>("AllViews")
                    .Customize(customization => customization.WaitForNonStaleResultsAsOfNow())
                    .Count();
            }

            if (resultViewCount > 0)
                throw new Exception(string.Format(
                    "Failed to delete all views. Initial view count: {0}, remaining view count: {1}.",
                    initialViewCount, resultViewCount));

            UpdateStatusMessage(string.Format("{0} views were deleted.", initialViewCount));
        }

        private void RepublishAllEvents()
        {
            int processedEventsCount = 0;
            int failedEventsCount = 0;

            ThrowIfShouldStopViewsRebuilding();

            UpdateStatusMessage("Determining count of events to be republished.");

            int allEventsCount = this.eventStore.CountOfAllEventsIncludingSnapshots();

            DateTime republishStarted = DateTime.Now;
            UpdateStatusMessage(
                "Acquiring first portion of events. "
                + GetReadablePublishingDetails(republishStarted, processedEventsCount, allEventsCount, failedEventsCount));

            foreach (CommittedEvent[] eventBulk in this.eventStore.GetAllEventsIncludingSnapshots())
            {
                foreach (CommittedEvent @event in eventBulk)
                {
                    ThrowIfShouldStopViewsRebuilding();

                    UpdateStatusMessage(
                        string.Format("Publishing event {0}. ", processedEventsCount + 1)
                        + GetReadablePublishingDetails(republishStarted, processedEventsCount, allEventsCount, failedEventsCount));

                    try
                    {
                        this.eventBus.Publish(@event);
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
                    + GetReadablePublishingDetails(republishStarted, processedEventsCount, allEventsCount, failedEventsCount));
            }

            UpdateStatusMessage(string.Format("All events were republished. "
                + GetReadablePublishingDetails(republishStarted, processedEventsCount, allEventsCount, failedEventsCount)));
        }

        private static string GetReadablePublishingDetails(DateTime republishStarted,
            int processedEventsCount, int allEventsCount, int failedEventsCount)
        {
            TimeSpan republishTimeSpent = DateTime.Now - republishStarted;

            int speedInEventsPerMinute = (int)(
                republishTimeSpent.TotalSeconds == 0
                ? 0
                : 60 * processedEventsCount / republishTimeSpent.TotalSeconds);

            return string.Format(
                "Processed events: {1}. Total events: {2}. Failed events: {3}.{0}Time spent republishing: {4}. Speed: {5} events per minute.",
                Environment.NewLine,
                processedEventsCount, allEventsCount, failedEventsCount,
                republishTimeSpent.ToString(@"hh\:mm\:ss"), speedInEventsPerMinute);
        }

        private static void ThrowIfShouldStopViewsRebuilding()
        {
            if (shouldStopViewsRebuilding)
            {
                shouldStopViewsRebuilding = false;
                throw new Exception("Views rebuilding stopped by request.");
            }
        }

        private static void UpdateStatusMessage(string newMessage)
        {
            statusMessage = string.Format("{0}: {1}", DateTime.Now, newMessage);
        }

        private string GetReadableListOfWriters()
        {
            List<IRavenReadSideRepositoryWriter> writers = this.writerRegistry.GetAll().ToList();

            bool areThereNoWriters = writers.Count == 0;

            return areThereNoWriters
                ? "Registered writers: None"
                : string.Format(
                    "Registered writers: {1}{0}{2}",
                    Environment.NewLine,
                    writers.Count,
                    string.Join(
                        Environment.NewLine,
                        writers
                            .Select(writer => writer.GetType().ToString())
                            .OrderBy(_ => _)
                            .ToArray()));
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
                shouldShowStackTrace ? error.Item3.ToString() : error.Item3.Message);
        }

        #endregion // Error reporting methods
    }
}