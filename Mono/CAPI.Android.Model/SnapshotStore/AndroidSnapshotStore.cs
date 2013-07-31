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
            if (!Directory.Exists(SnapshotStoreDirPath))
                Directory.CreateDirectory(SnapshotStoreDirPath);
        }

        private const string snapshotTemp = "snapshotTemp";
        protected string SnapshotStoreDirPath {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), snapshotTemp);
            }
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
            var inMemorySnapshot = this.GetSnapshot(eventSourceId, maxVersion);
            if (inMemorySnapshot != null)
                return inMemorySnapshot;


            var filePath = GetFileName(eventSourceId);
            if (!File.Exists(filePath))
                return null;
            var snapshot = JsonUtils.GetObject<Snapshot>(File.ReadAllText(filePath));
            this.SaveShapshot(snapshot);
            return snapshot;
        }

        public void FlushSnapshot(Guid eventSourceId)
        {
            var snapshot = this.TryGetSnapshot(eventSourceId, long.MaxValue);
            if(snapshot==null)
                return;

            var path = GetFileName(eventSourceId);
            File.WriteAllText(path, JsonUtils.GetJsonData(snapshot));

            //here we have to Remove item from snapshot store
            //to minimize memory usage
            _snapshots.Remove(eventSourceId);
        }

        public void DeleteSnapshot(Guid eventSourceId)
        {
            var path = GetFileName(eventSourceId);
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            //here we have to Remove item from snapshot store
            //to minimize memory usage
            _snapshots.Remove(eventSourceId);
        }

        private string GetFileName(Guid id)
        {
            return Path.Combine(SnapshotStoreDirPath,id.ToString());
        }

        public string GetPathToBakupFile()
        {
            return null;
        }

        public void RestoreFromBakupFolder(string path)
        {
            foreach (var file in Directory.EnumerateFiles(SnapshotStoreDirPath))
            {
                File.Delete(file);
            }

            _snapshots = new Dictionary<Guid, Snapshot>();
        }
    }
}