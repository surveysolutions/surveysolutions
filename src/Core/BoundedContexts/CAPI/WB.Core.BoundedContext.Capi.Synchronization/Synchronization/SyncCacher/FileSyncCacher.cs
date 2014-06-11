using System;
using System.IO;
using WB.Core.Infrastructure.Backup;

namespace WB.Core.BoundedContext.Capi.Synchronization.Synchronization.SyncCacher
{
    public class FileSyncCacher : ISyncCacher, IBackupable
    {
        private const string CacheFolder = "SyncCache";

        private readonly string _basePath;

        public FileSyncCacher()
        {
            this._basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), CacheFolder);
            if (!Directory.Exists(this._basePath))
            {
                Directory.CreateDirectory(this._basePath);
            }
        }

        public bool SaveItem(Guid itemId, string itemContent)
        {
            File.WriteAllText(this.BuildFileName(itemId.ToString()), itemContent);
            return true;
        }

        public string LoadItem(Guid itemId)
        {
            var longFileName = this.BuildFileName(itemId.ToString());
            if (File.Exists(longFileName))
                return File.ReadAllText(longFileName);
            return null;
        }

        public bool DoesCachedItemExist(Guid itemId)
        {
            var longFileName = this.BuildFileName(itemId.ToString());
            return File.Exists(longFileName);
        }

        public bool DeleteItem(Guid itemId)
        {
            var longFileName = this.BuildFileName(itemId.ToString());
            if (File.Exists(longFileName))
                File.Delete(this.BuildFileName(longFileName));

            return true;
        }


        private string BuildFileName(string fileName)
        {
            return Path.Combine(this._basePath, fileName);
        }

        public string GetPathToBackupFile()
        {
            return this._basePath;
        }

        public void RestoreFromBackupFolder(string path)
        {
            var dirWithCahngelog = Path.Combine(path, CacheFolder);
            
            foreach (var file in Directory.EnumerateFiles(this._basePath))
            {
                File.Delete(file);
            }

            if (!Directory.Exists(dirWithCahngelog))
                return;

            foreach (var file in Directory.GetFiles(dirWithCahngelog))
                File.Copy(file, Path.Combine(this._basePath, Path.GetFileName(file)));
        }
    }
}