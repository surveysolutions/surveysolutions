using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using Ncqrs.Restoring.EventStapshoot.EventStores;

namespace Ncqrs.Restoring.EventStapshoot
{
    public class InMemorySnapshootStore : ISnapshotStore
    {
        private readonly ISnapshotStore snapshootStore;
        private ISnapshootEventStore eventStore;

        public InMemorySnapshootStore(ISnapshootEventStore eventStore, ISnapshotStore snapshotStore)
        {
            this.snapshootStore = snapshotStore;
            this.eventStore = eventStore;
        }

        public void SaveShapshot(Snapshot snapshot)
        {
            this.snapshootStore.SaveShapshot(snapshot);
        }

        public Snapshot GetSnapshot(Guid eventSourceId, long maxVersion)
        {
            var snapshoot = this.snapshootStore.GetSnapshot(eventSourceId, maxVersion);
            if (snapshoot != null)
                return snapshoot;
            var snapshootLoaded = this.eventStore.GetLatestSnapshoot(eventSourceId);
            if (snapshootLoaded != null)
            {
                snapshoot = (snapshootLoaded.Payload as SnapshootLoaded).Template;
                var newSnapshot = new Snapshot(eventSourceId, snapshootLoaded.EventSequence, snapshoot.Payload);

                this.snapshootStore.SaveShapshot(newSnapshot);
                return newSnapshot;
            }
            return null;
        }
    }
}
