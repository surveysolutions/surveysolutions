using System;
using System.IO;
using Main.Core.Events;
using Newtonsoft.Json;
using SynchronizationMessages.Synchronization;

namespace CAPI.Android.Core.Model.ChangeLog
{
    public class FileChangeLogStore : IChangeLogStore
    {
        public FileChangeLogStore()
        {
        }

        public void SaveChangeset(AggregateRootEvent[] recordData, Guid recordId)
        {
            var path = GetFileName(recordId);
            using (var fs = File.Open(path, FileMode.CreateNew))
            {
                var bytes = PackageHelper.Compress(GetJsonData(recordData));
                fs.Write(bytes, 0, bytes.Length);
            }
        }

        public void DeleteDraftChangeSet(Guid recordId)
        {
            var path = GetFileName(recordId);
            File.Delete(path);
        }

        private string GetFileName(Guid publicKey)
        {
            return System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
                                          publicKey.ToString());
        }


        private string GetJsonData(AggregateRootEvent[] payload)
        {
            var data = JsonConvert.SerializeObject(payload, Formatting.None,
                                                   new JsonSerializerSettings
                                                       {
                                                           TypeNameHandling = TypeNameHandling.Objects,
                                                           NullValueHandling = NullValueHandling.Ignore
                                                       });
            return data;
        }
    }
}