using System;
using System.Collections.Generic;
using System.IO;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Capi.ModelUtils;
using WB.Core.Infrastructure.Backup;

namespace CAPI.Android.Core.Model.SnapshotStore
{
    public class FileBasedSnapshotStore : ISnapshotStore, IBackupable
    {
        private Dictionary<Guid, Snapshot> snapshots = new Dictionary<Guid, Snapshot>();
        private const string PersistingFolder = "SnapshotStore";
        private readonly string basePath;

        public FileBasedSnapshotStore()
        {
            this.basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), PersistingFolder);
            if (!Directory.Exists(this.basePath))
            {
                Directory.CreateDirectory(this.basePath);
            }
        }

        public void SaveShapshot(Snapshot snapshot)
        {
            this.snapshots[snapshot.EventSourceId] = snapshot;
        }

        public void PersistShapshot(Guid eventSourceId)
        {
            if (!this.snapshots.ContainsKey(eventSourceId))
                return;
            var snapshot = this.snapshots[eventSourceId];
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
            return Path.Combine(this.basePath, fileName);
        }

        public Snapshot TryGetSnapshot(Guid eventSourceId, long maxVersion)
        {
            Snapshot snapshot = null;
            if (this.snapshots.ContainsKey(eventSourceId))
            {
                snapshot = this.snapshots[eventSourceId];
            }
            else
            {
                snapshot = GetSnapshotFromString(LoadItem(eventSourceId));
                if (snapshot == null)
                    return null;
                this.snapshots[eventSourceId] = snapshot;
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
            this.snapshots.Remove(eventSourceId);
            this.DeleteItem(eventSourceId);
        }

        public string GetPathToBackupFile()
        {
            return null;
        }

        public void RestoreFromBackupFolder(string path)
        {
            this.snapshots = new Dictionary<Guid, Snapshot>();

            foreach (var file in Directory.EnumerateFiles(this.basePath))
            {
                File.Delete(file);
            }
        }
    }
}