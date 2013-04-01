using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using Ncqrs.Restoring.EventStapshoot.EventStores;

namespace Ncqrs.Restoring.EventStapshoot
{
    public class InMemorySnapshootStore : ISnapshotStore
    {
        private readonly ISnapshotStore uncommitedSnapshootStore;
       // private readonly ISnapshotStore commitedSnapshootStore; 
        private ISnapshootEventStore eventStore;

        public InMemorySnapshootStore(ISnapshootEventStore eventStore)
            : this(eventStore, new InMemoryEventStore())
        {
        }
        public InMemorySnapshootStore(ISnapshootEventStore eventStore, ISnapshotStore uncommitedSnapshootStore)
        {
            this.uncommitedSnapshootStore = uncommitedSnapshootStore;
            this.eventStore = eventStore;
        }
       /* public InMemorySnapshootStore(ISnapshootEventStore eventStore, ISnapshotStore uncommitedSnapshootStore, ISnapshotStore commitedSnapshootStore)
        {
            this.uncommitedSnapshootStore = uncommitedSnapshootStore;
        //    this.commitedSnapshootStore = commitedSnapshootStore; 
            this.eventStore = eventStore;
        }*/
        public void SaveShapshot(Snapshot snapshot)
        {
          /*  var commitedSnapshoot = this.commitedSnapshootStore.GetSnapshot(snapshot.EventSourceId, long.MaxValue);
            if (commitedSnapshoot == null || commitedSnapshoot.Version < snapshot.Version)*/
                this.uncommitedSnapshootStore.SaveShapshot(snapshot);
        }

        public Snapshot GetSnapshot(Guid eventSourceId, long maxVersion)
        {
            var uncommitedSnapshoot = this.uncommitedSnapshootStore.GetSnapshot(eventSourceId, maxVersion);
            if (uncommitedSnapshoot != null)
                return uncommitedSnapshoot;
       /*     var commitedSnapshoot = this.commitedSnapshootStore.GetSnapshot(eventSourceId, maxVersion);
            if (commitedSnapshoot != null)
                return commitedSnapshoot;*/
            var snapshootLoaded = this.eventStore.GetLatestSnapshoot(eventSourceId);
            if (snapshootLoaded != null)
            {
                var commitedSnapshoot = SaveEventToSnapshotStore(snapshootLoaded);
                return commitedSnapshoot;
            }
            return null;
        }


        public Snapshot SaveEventToSnapshotStore(CommittedEvent evt)
        {
            var snapshoot = (evt.Payload as SnapshootLoaded).Template;
            var newSnapshot = new CommitedSnapshot(evt.EventSourceId, evt.EventSequence, snapshoot.Payload);

            SaveShapshot(newSnapshot);
            return newSnapshot;
        }
    }
}
