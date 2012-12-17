// -----------------------------------------------------------------------
// <copyright file="EventSyncProcess.cs" company="The World Bank">
// Event Sync Process
// </copyright>
// -----------------------------------------------------------------------

namespace DataEntryClient.CompleteQuestionnaire
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Events;

    using Ninject;

    using SynchronizationMessages.CompleteQuestionnaire;

    /// <summary>
    /// Event sync process
    /// </summary>
    public interface IEventSyncProcess : ISyncProcess
    {
        /// <summary>
        /// The import
        /// </summary>
        /// <param name="syncProcessDescription">
        /// The sync process description.
        /// </param>
        /// <param name="events">
        /// The events.
        /// </param>
        void Import(string syncProcessDescription, IEnumerable<AggregateRootEvent> events);

        /// <summary>
        /// Export events
        /// </summary>
        /// <param name="syncProcessDescription">
        /// The sync Process Description.
        /// </param>
        /// <param name="firstEventPulicKey">
        /// The first Event Pulic Key.
        /// </param>
        /// <param name="length">
        /// The length.
        /// </param>
        /// <returns>
        /// ImportSynchronizationMessage file with events
        /// </returns>
        ImportSynchronizationMessage Export(string syncProcessDescription, Guid firstEventPulicKey, int length);

        /// <summary>
        /// Export events
        /// </summary>
        /// <param name="syncProcessDescription">
        /// The sync Process Description.
        /// </param>
        /// <returns>
        /// List Of Aggregate Roots For Import Message 
        /// </returns>
        ListOfAggregateRootsForImportMessage Export(string syncProcessDescription);
    }

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class EventSyncProcess : AbstractSyncProcess, IEventSyncProcess
    {
        /// <summary>
        /// List of income events
        /// </summary>
        private IEnumerable<AggregateRootEvent> incomeEvents;

        /// <summary>
        /// Event Pipe Collector
        /// </summary>
        private EventPipeCollector collector;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventSyncProcess"/> class.
        /// </summary>
        /// <param name="kernel">
        /// The kernel.
        /// </param>
        /// <param name="syncProcess">
        /// The sync process.
        /// </param>
        /// <param name="parentSyncProcess">
        /// The parent Sync Process.
        /// </param>
        public EventSyncProcess(IKernel kernel, Guid syncProcess, Guid? parentSyncProcess = null)
            : base(kernel, syncProcess, parentSyncProcess)
        {
        }

        /// <summary>
        /// The import
        /// </summary>
        /// <param name="syncProcessDescription">
        /// The sync process description.
        /// </param>
        [Obsolete("Import(string) is deprecated, please use Import(string, IEnumerable<AggregateRootEvent>) instead.", true)]
        public new void Import(string syncProcessDescription)
        {
        }

        /// <summary>
        /// The import
        /// </summary>
        /// <param name="syncProcessDescription">
        /// The sync process description.
        /// </param>
        /// <param name="events">
        /// The events.
        /// </param>
        public void Import(string syncProcessDescription, IEnumerable<AggregateRootEvent> events)
        {
            this.incomeEvents = events;
            base.Import(syncProcessDescription);
        }

        /// <summary>
        /// Export events
        /// </summary>
        /// <param name="syncProcessDescription">
        /// The sync Process Description.
        /// </param>
        /// <param name="firstEventPulicKey">
        /// The first Event Pulic Key.
        /// </param>
        /// <param name="length">
        /// The length.
        /// </param>
        /// <returns>
        /// ImportSynchronizationMessage file with events
        /// </returns>
        public ImportSynchronizationMessage Export(string syncProcessDescription, Guid firstEventPulicKey, int length)
        {
            this.collector = new EventPipeCollector();
            base.Export(syncProcessDescription);
            return new ImportSynchronizationMessage
                {
                    EventStream = this.collector.GetEventList()
                                    .SkipWhile(e => e.EventIdentifier != firstEventPulicKey)
                                    .Take(length)
                                    .ToArray()
                };
        }

        /// <summary>
        /// Export events
        /// </summary>
        /// <param name="syncProcessDescription">
        /// The sync Process Description.
        /// </param>
        /// <returns>
        /// List Of Aggregate Roots For Import Message 
        /// </returns>
        public new ListOfAggregateRootsForImportMessage Export(string syncProcessDescription)
        {
            this.collector = new EventPipeCollector();
            base.Export(syncProcessDescription);
            return new ListOfAggregateRootsForImportMessage
                {
                   Roots = this.collector.GetChunkedList().Select(e => new ProcessedEventChunk(e)).ToList() 
                };
        }

        /// <summary>
        /// The stub
        /// </summary>
        protected override void ExportEvents()
        {
            this.ProcessEvents(this.collector);
        }

        /// <summary>
        /// Gets events
        /// </summary>
        /// <returns>
        /// List of AggregateRootEvent
        /// </returns>
        protected override IEnumerable<AggregateRootEvent> GetEventStream()
        {
            return this.incomeEvents;
        }
    }
}
