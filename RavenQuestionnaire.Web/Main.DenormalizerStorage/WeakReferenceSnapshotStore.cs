// -----------------------------------------------------------------------
// <copyright file="WeakReferenceSnapshotStore.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;

namespace Main.DenormalizerStorage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class WeakReferenceSnapshotStore : ISnapshotStore
    {
        private readonly PersistentDenormalizer<Snapshot> storage;

        public WeakReferenceSnapshotStore(PersistentDenormalizer<Snapshot> storage)
        {
            this.storage = storage;
        }

        #region Implementation of ISnapshotStore

        public void SaveShapshot(Snapshot snapshot)
        {
            storage.Store(snapshot, snapshot.EventSourceId);
        }

        public Snapshot GetSnapshot(Guid eventSourceId, long maxVersion)
        {
            return storage.GetById(eventSourceId);


        }
        #endregion
    }
}
