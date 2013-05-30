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
        void Import(string syncProcessDescription, IEnumerable<AggregateRootEvent> events);

        ImportSynchronizationMessage Export(string syncProcessDescription, Guid firstEventPublicKey, int length);

        ListOfAggregateRootsForImportMessage Export(string syncProcessDescription);

        SyncItemsMetaContainer GetListOfAggregateRoots(string syncProcessDescription);

        ImportSynchronizationMessage GetAR(string syncProcessDescription, Guid firstEventPublicKey, string ARType ,int length);

    }
}