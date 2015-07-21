using System;
using System.Collections.Generic;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;

namespace WB.Core.Infrastructure.Implementation.Storage
{
    public class InMemoryCachedSnapshotStore : ISnapshotStore
    {
        private readonly int Capacity = 200;

        private readonly Dictionary<Guid, Snapshot> snapshots = new Dictionary<Guid, Snapshot>();
        private readonly LinkedList<Guid> list = new LinkedList<Guid>();
        private readonly object Lock = new object();


        public void SaveShapshot(Snapshot snapshot)
        {
           lock (Lock)
           {
                if (snapshots.ContainsKey(snapshot.EventSourceId))
                {
                    list.Remove(snapshot.EventSourceId);
                    list.AddLast(snapshot.EventSourceId);
                    snapshots[snapshot.EventSourceId] = snapshot;
                    return;
                }

                if (snapshots.Count >= Capacity && Capacity != 0)
                {
                    var itemToRemove = list.First.Value;
                    list.Remove(itemToRemove);
                    if (snapshots.ContainsKey(itemToRemove))
                    {
                        snapshots.Remove(itemToRemove);
                    }
                }

               list.AddLast(snapshot.EventSourceId);
               snapshots.Add(snapshot.EventSourceId, snapshot);
           }
        }

        public Snapshot GetSnapshot(Guid eventSourceId, int maxVersion)
        {
            Snapshot result = null;
            lock (Lock)
            {
                if (!snapshots.ContainsKey(eventSourceId))
                    return null;

                result = snapshots[eventSourceId];
            }

            return result.Version > maxVersion 
                ? null 
                : result;
        }
    }
}
