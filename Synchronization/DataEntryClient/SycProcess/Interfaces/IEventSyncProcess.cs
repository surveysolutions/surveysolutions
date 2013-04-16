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
        /// The export.
        /// </summary>
        /// <param name="syncProcessDescription">
        /// The sync process description.
        /// </param>
        /// <param name="firstEventPublicKey">
        /// The first event public key.
        /// </param>
        /// <param name="length">
        /// The length.
        /// </param>
        /// <returns>
        /// The <see cref="ImportSynchronizationMessage"/>.
        /// </returns>
        ImportSynchronizationMessage Export(string syncProcessDescription, Guid firstEventPublicKey, int length);

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


        SyncItemsMetaContainer GetListOfAggregateRoots(string syncProcessDescription);


        ImportSynchronizationMessage GetAR(string syncProcessDescription, Guid firstEventPublicKey, string ARType ,int length);

    }
}