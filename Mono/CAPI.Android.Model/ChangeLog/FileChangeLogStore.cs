using System;
using System.IO;
using CAPI.Android.Core.Model.ModelUtils;
using Main.Core;
using Main.Core.Events;

namespace CAPI.Android.Core.Model.ChangeLog
{
    public class FileChangeLogStore : IChangeLogStore
    {
        private const string ChangelogFolder = "Changelog";
        private readonly string changelogPath;

        public FileChangeLogStore()
        {
            changelogPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), ChangelogFolder);
            if (!Directory.Exists(changelogPath))
            {
                Directory.CreateDirectory(changelogPath);
            }
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
            return System.IO.Path.Combine(changelogPath,
                                          publicKey.ToString());
        }


        public string GetPathToBakupFile()
        {
            return changelogPath;
        }

        public void RestoreFromBakupFolder(string path)
        {
            var dirWithCahngelog = Path.Combine(path, ChangelogFolder);
            foreach (var file in Directory.EnumerateFiles(changelogPath))
            {
                File.Delete(file);
            }

            foreach (var file in Directory.GetFiles(dirWithCahngelog))
                File.Copy(file, Path.Combine(changelogPath, Path.GetFileName(file)));
        }
    }
}