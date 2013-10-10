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
        private const string PersistingFolder = "SnapshotStore";
        private readonly string _basePath;

        public AndroidSnapshotStore()
        {
            _basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), PersistingFolder);
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }

        public void SaveShapshot(Snapshot snapshot)
        {
            _snapshots[snapshot.EventSourceId] = snapshot;
        }

        public void PersistShapshot(Guid eventSourceId)
        {
            if (!_snapshots.ContainsKey(eventSourceId))
                return;
            var snapshot = _snapshots[eventSourceId];
            try
            {
                SaveItem(eventSourceId, JsonUtils.GetJsonData(snapshot));
            }
            catch (Exception)
            {
            }
            
        }

        private bool SaveItem(Guid itemId, string itemContent)
        {
            File.WriteAllText(BuildFileName(itemId.ToString()), itemContent);
            return true;
        }

        private bool DeleteItem(Guid itemId)
        {
            var longFileName = BuildFileName(itemId.ToString());
            if (File.Exists(longFileName))
                File.Delete(BuildFileName(longFileName));

            return true;
        }

        private string LoadItem(Guid itemId)
        {
            var longFileName = BuildFileName(itemId.ToString());
            if (File.Exists(longFileName))
                return File.ReadAllText(longFileName);
            return null;
        }

        private Snapshot GetSnapshotFromString(string item)
        {
            Snapshot snapshot = null;
            if (!string.IsNullOrWhiteSpace(item))
            {
                try
                {
                    snapshot = JsonUtils.GetObject<Snapshot>(item);
                }
                catch (Exception)
                {
                }
            }
            return snapshot;
        }

        private string BuildFileName(string fileName)
        {
            return Path.Combine(_basePath, fileName);
        }

        public Snapshot TryGetSnapshot(Guid eventSourceId, long maxVersion)
        {
            Snapshot snapshot = null;
            if (_snapshots.ContainsKey(eventSourceId))
            {
                snapshot = _snapshots[eventSourceId];
            }
            else
            {
                snapshot = GetSnapshotFromString(LoadItem(eventSourceId));
                if (snapshot == null)
                    return null;
                _snapshots[eventSourceId] = snapshot;
            }
            
            return snapshot.Version > maxVersion 
                ? null 
                : snapshot;
        }

        public Snapshot GetSnapshot(Guid eventSourceId, long maxVersion)
        {
            return this.TryGetSnapshot(eventSourceId, maxVersion);
        }

        public void DeleteSnapshot(Guid eventSourceId)
        {
            _snapshots.Remove(eventSourceId);
            this.DeleteItem(eventSourceId);
        }

        public string GetPathToBakupFile()
        {
            return null;
        }

        public void RestoreFromBakupFolder(string path)
        {
            _snapshots = new Dictionary<Guid, Snapshot>();

            var dirWithCahngelog = Path.Combine(path, _basePath);
            foreach (var file in Directory.EnumerateFiles(_basePath))
            {
                File.Delete(file);
            }

            foreach (var file in Directory.GetFiles(dirWithCahngelog))
                File.Copy(file, Path.Combine(_basePath, Path.GetFileName(file)));
        }
    }
}