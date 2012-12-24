namespace DataEntryClient.SycProcess.Interfaces
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Events;

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
}