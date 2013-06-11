using System;
using System.IO;
using CAPI.Android.Core.Model.ModelUtils;
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
            File.WriteAllText(path, PackageHelper.CompressString(JsonUtils.GetJsonData(recordData)));
        }

        public string GetChangesetContent(Guid recordId)
        {
            var path = GetFileName(recordId);
            if (!File.Exists(path))
                return string.Empty;
            return File.ReadAllText(path);
        }

        public void DeleteDraftChangeSet(Guid recordId)
        {
            var path = GetFileName(recordId);
            if (File.Exists(path))
                File.Delete(path);
        }

        private string GetFileName(Guid publicKey)
        {
            return System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
                                          publicKey.ToString());
        }


    }
}