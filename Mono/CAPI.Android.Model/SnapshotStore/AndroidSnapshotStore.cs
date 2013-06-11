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
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using Newtonsoft.Json;

namespace CAPI.Android.Core.Model.SnapshotStore
{
    public class AndroidSnapshotStore : ISnapshotStore
    {
        public AndroidSnapshotStore()
        {
            internalStorage = new InMemoryEventStore();
            var dirPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), snapshotTemp);
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
        }

        private readonly ISnapshotStore internalStorage;
        private const string snapshotTemp = "snapshotTemp";

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
            return GetObject(File.ReadAllText(filePath));
        }

        public void Flush(Guid eventSourceId)
        {
            var snapshot = internalStorage.GetSnapshot(eventSourceId, long.MaxValue);
            if(snapshot==null)
                return;

            var path = GetFileName(eventSourceId);
            File.WriteAllText(path, GetJsonData(snapshot));
        }

        private string GetFileName(Guid id)
        {
            return System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), snapshotTemp,
                                          id.ToString());
        }

        private string GetJsonData(object payload)
        {
            var data = JsonConvert.SerializeObject(payload, Formatting.None,
                                                   new JsonSerializerSettings
                                                   {
                                                       TypeNameHandling = TypeNameHandling.Objects
                                                   });
            
            return data;
        }

        private Snapshot GetObject(string json) 
        {
            return JsonConvert.DeserializeObject<Snapshot>(json,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects
                });
        }
    }
}