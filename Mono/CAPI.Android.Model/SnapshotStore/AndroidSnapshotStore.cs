using System;
using System.Collections.Generic;
using System.IO;
using CAPI.Android.Core.Model.ModelUtils;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.Backup;

namespace CAPI.Android.Core.Model.SnapshotStore
{
    public class AndroidSnapshotStore : ISnapshotStore, IBackupable
    {
        private Dictionary<Guid, Snapshot> _snapshots = new Dictionary<Guid, Snapshot>();

        public AndroidSnapshotStore()
        {
        }

        public void SaveShapshot(Snapshot snapshot)
        {
            _snapshots[snapshot.EventSourceId] = snapshot;
        }
        
        public Snapshot TryGetSnapshot(Guid eventSourceId, long maxVersion)
        {
            if (!_snapshots.ContainsKey(eventSourceId))
                return null;
            var result = _snapshots[eventSourceId];
            return result.Version > maxVersion 
                ? null 
                : result;
        }

        public Snapshot GetSnapshot(Guid eventSourceId, long maxVersion)
        {
            return this.TryGetSnapshot(eventSourceId, maxVersion);
        }

        public void DeleteSnapshot(Guid eventSourceId)
        {
            _snapshots.Remove(eventSourceId);
        }

        public string GetPathToBakupFile()
        {
            return null;
        }

        public void RestoreFromBakupFolder(string path)
        {

            _snapshots = new Dictionary<Guid, Snapshot>();
        }
    }
}