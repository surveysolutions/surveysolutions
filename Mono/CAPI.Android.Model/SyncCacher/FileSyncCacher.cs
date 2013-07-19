using System;
using System.IO;

namespace CAPI.Android.Core.Model.SyncCacher
{
    public class FileSyncCacher : ISyncCacher
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
            DeleteItem(itemId);
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

    }
}