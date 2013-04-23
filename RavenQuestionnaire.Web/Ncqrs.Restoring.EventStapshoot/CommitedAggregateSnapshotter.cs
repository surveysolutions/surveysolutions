using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ncqrs.Domain;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing.Snapshotting;

namespace Ncqrs.Restoring.EventStapshoot
{
    public class CommitedAggregateSnapshotter : IAggregateSnapshotter
    {
        private readonly IAggregateSnapshotter subSnapshotter;

        public CommitedAggregateSnapshotter(IAggregateSnapshotter subSnapshotter)
        {
            this.subSnapshotter = subSnapshotter;
        }

        public bool TryLoadFromSnapshot(Type aggregateRootType, Snapshot snapshot, CommittedEventStream committedEventStream,
                                        out AggregateRoot aggregateRoot)
        {
            return this.subSnapshotter.TryLoadFromSnapshot(aggregateRootType, snapshot, committedEventStream,
                                                           out aggregateRoot);
        }

        public bool TryTakeSnapshot(AggregateRoot aggregateRoot, out Snapshot snapshot)
        {
             var snapshotResult=this.subSnapshotter.TryTakeSnapshot(aggregateRoot, out snapshot);
               Type arType = aggregateRoot.GetType();
            if (!snapshotResult)
                return false;
            var snapshotables = from i in arType.GetInterfaces()
                                where i.IsGenericType && i.GetGenericTypeDefinition() == typeof (ISnapshotable<>)
                                select i;
            if (!snapshotables.Any())
            {
                return true;
            }
            if (!typeof (SnapshootableAggregateRoot<>).MakeGenericType(
                snapshotables.FirstOrDefault().GetGenericArguments()[0]).IsInstanceOfType(aggregateRoot))
            {
                return true;
            }
            var lastPersistedSnapshotProp = arType.GetProperty("LastPersistedSnapshot");
            var lastPersistedSnapshotVal = (long?) lastPersistedSnapshotProp.GetValue(aggregateRoot, new object[0]);
            if(lastPersistedSnapshotVal.HasValue && lastPersistedSnapshotVal.Value==snapshot.Version)
                snapshot = new CommitedSnapshot(snapshot.EventSourceId, snapshot.Version, snapshot.Payload);
            return true;
        }
    }
}
