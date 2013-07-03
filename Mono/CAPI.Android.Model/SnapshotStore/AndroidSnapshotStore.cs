using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core.Model.ModelUtils;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using Newtonsoft.Json;
using WB.Core.Infrastructure.Backup;

namespace CAPI.Android.Core.Model.SnapshotStore
{
    public class AndroidSnapshotStore : ISnapshotStore, IBackupable
    {
        public AndroidSnapshotStore()
        {
            internalStorage = new InMemoryEventStore();

            if (!Directory.Exists(SnapshotStoreDirPath))
                Directory.CreateDirectory(SnapshotStoreDirPath);
        }

        private readonly ISnapshotStore internalStorage;
        private const string snapshotTemp = "snapshotTemp";
        protected string SnapshotStoreDirPath {
            get
            {
                return
                    System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
                                           snapshotTemp);
            }
        }

        public void SaveShapshot(Snapshot snapshot)
        {
            internalStorage.SaveShapshot(snapshot);
        }

        public Snapshot GetSnapshot(Guid eventSourceId, long maxVersion)
        {
            var inMemorySnapshot = internalStorage.GetSnapshot(eventSourceId, maxVersion);
            if (inMemorySnapshot != null)
                return inMemorySnapshot;


            var filePath = GetFileName(eventSourceId);
            if (!File.Exists(filePath))
                return null;
            var snapshot = JsonUtils.GetObject<Snapshot>(File.ReadAllText(filePath));
            internalStorage.SaveShapshot(snapshot);
            return snapshot;
        }

        public void Flush(Guid eventSourceId)
        {
            var snapshot = internalStorage.GetSnapshot(eventSourceId, long.MaxValue);
            if(snapshot==null)
                return;

            var path = GetFileName(eventSourceId);
            File.WriteAllText(path, JsonUtils.GetJsonData(snapshot));
        }

        private string GetFileName(Guid id)
        {
            return System.IO.Path.Combine(SnapshotStoreDirPath,
                                          id.ToString());
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
        }
    }
}