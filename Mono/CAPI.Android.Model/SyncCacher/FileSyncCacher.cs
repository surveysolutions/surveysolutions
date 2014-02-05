using System;
using System.IO;
using WB.Core.Infrastructure.Backup;

namespace CAPI.Android.Core.Model.SyncCacher
{
    public class FileSyncCacher : ISyncCacher, IBackupable
    {
        private const string CacheFolder = "SyncCache";

        private readonly string _basePath;

        public FileSyncCacher()
        {
            _basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), CacheFolder);
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }

        public bool SaveItem(Guid itemId, string itemContent)
        {
            File.WriteAllText(BuildFileName(itemId.ToString()), itemContent);
            return true;
        }

        public string LoadItem(Guid itemId)
        {
            var longFileName = BuildFileName(itemId.ToString());
            if (File.Exists(longFileName))
                return File.ReadAllText(longFileName);
            return null;
        }

        public bool DoesCachedItemExist(Guid itemId)
        {
            var longFileName = BuildFileName(itemId.ToString());
            return File.Exists(longFileName);
        }

        public bool DeleteItem(Guid itemId)
        {
            var longFileName = BuildFileName(itemId.ToString());
            if (File.Exists(longFileName))
                File.Delete(BuildFileName(longFileName));

            return true;
        }


        private string BuildFileName(string fileName)
        {
            return Path.Combine(_basePath, fileName);
        }

        public string GetPathToBackupFile()
        {
            return _basePath;
        }

        public void RestoreFromBackupFolder(string path)
        {
            var dirWithCahngelog = Path.Combine(path, CacheFolder);
            
            foreach (var file in Directory.EnumerateFiles(_basePath))
            {
                File.Delete(file);
            }

            if (!Directory.Exists(dirWithCahngelog))
                return;

            foreach (var file in Directory.GetFiles(dirWithCahngelog))
                File.Copy(file, Path.Combine(_basePath, Path.GetFileName(file)));
        }
    }
}