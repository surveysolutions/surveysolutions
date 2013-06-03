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
using Raven.Client.Indexes;

namespace WB.Core.Infrastructure.Implementation
{
    internal class ReadLayerService : IReadLayerStatusService, IReadLayerAdministrationService
    {
        private const int MaxAllowedFailedEvents = 100;

        private static readonly object RebuildAllViewsLockObject = new object();
        private static readonly object ErrorsLockObject = new object();

        private static bool areViewsBeingRebuiltNow = false;

        private static string statusMessage;
        private static List<Tuple<DateTime, string, Exception>> errors = new List<Tuple<DateTime,string,Exception>>();

        private readonly IStreamableEventStore eventStore;
        private readonly IEventBus eventBus;
        private readonly DocumentStore ravenStore;

        static ReadLayerService()
        {
            UpdateStatusMessage("No administration operations were performed so far.");
        }

        public ReadLayerService(IStreamableEventStore eventStore, IEventBus eventBus, DocumentStore ravenStore)
        {
            this.eventStore = eventStore;
            this.eventBus = eventBus;
            this.ravenStore = ravenStore;
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
            return string.Format("{1}{0}Are views being rebuilt now: {2}{0}{3}",
                Environment.NewLine,
                statusMessage,
                areViewsBeingRebuiltNow ? "Yes" : "No",
                GetReadableErrors());
        }

        public void RebuildAllViewsAsync()
        {
            new Task(this.RebuildAllViews).Start();
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
                SaveErrorForStatusReport("Unexpected error occurred", exception);
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

            UpdateStatusMessage("Determining count of events to be republished.");

            int allEventsCount = this.eventStore.CountOfAllEventsWithoutSnapshots();

            UpdateStatusMessage(string.Format("Preparing to republish {0} events.", allEventsCount));

            foreach (CommittedEvent @event in this.eventStore.GetAllEventsWithoutSnapshots())
            {
                UpdateStatusMessage(string.Format("Publishing event {0} of {1}. Failed events: {2}.",
                    processedEventsCount + 1, allEventsCount, failedEventsCount));

                try
                {
                    this.eventBus.Publish(@event);
                }
                catch (Exception exception)
                {
                    SaveErrorForStatusReport(
                        string.Format("Failed to publish event {0} of {1} ({2})",
                            processedEventsCount + 1, allEventsCount, @event.EventIdentifier),
                        exception);

                    failedEventsCount++;
                }

                processedEventsCount++;

                if (failedEventsCount >= MaxAllowedFailedEvents)
                    throw new Exception(string.Format("Failed to rebuild read layer. Too many events failed: {0}.", failedEventsCount));
            }

            UpdateStatusMessage(string.Format("{0} events were republished. Failed events: {1}.",
                processedEventsCount, failedEventsCount));
        }

        private static void UpdateStatusMessage(string newMessage)
        {
            statusMessage = string.Format("{0}: {1}", DateTime.Now, newMessage);
        }

        #region Error reporting methods

        private static void SaveErrorForStatusReport(string message, Exception exception)
        {
            lock (ErrorsLockObject)
            {
                errors.Add(Tuple.Create(DateTime.Now, message, exception));
            }
        }

        private static string GetReadableErrors()
        {
            lock (ErrorsLockObject)
            {
                bool areThereNoErrors = errors.Count == 0;
                bool shouldShowStackTrace = errors.Count < 10;

                return areThereNoErrors
                    ? "Errors: None"
                    : string.Format(
                        "Errors: {1}{0}{2}",
                        Environment.NewLine,
                        errors.Count,
                        string.Join(Environment.NewLine, errors.Select(error => GetReadableError(error, shouldShowStackTrace)).ToArray()));
            }
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