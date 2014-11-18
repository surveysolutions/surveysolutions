using System;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.Infrastructure.Backup;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Core.BoundedContexts.Capi.Implementation.Services
{
    public class FileCapiSynchronizationCacheService : ICapiSynchronizationCacheService, IBackupable
    {
        private const string CacheFolder = "SyncCache";
        private IFileSystemAccessor fileSystemAccessor;
        private readonly string _basePath;

        public FileCapiSynchronizationCacheService(IFileSystemAccessor fileSystemAccessor, string environmentalPersonalFolderPath)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this._basePath = fileSystemAccessor.CombinePath(environmentalPersonalFolderPath, CacheFolder);
            if (!fileSystemAccessor.IsDirectoryExists(this._basePath))
            {
                fileSystemAccessor.CreateDirectory(this._basePath);
            }
        }

        public bool SaveItem(Guid itemId, string itemContent)
        {
            fileSystemAccessor.WriteAllText(this.BuildFileName(itemId.ToString()), itemContent);
            return true;
        }

        public string LoadItem(Guid itemId)
        {
            var longFileName = this.BuildFileName(itemId.ToString());
            if (fileSystemAccessor.IsFileExists(longFileName))
                return fileSystemAccessor.ReadAllText(longFileName);
            return null;
        }

        public bool DoesCachedItemExist(Guid itemId)
        {
            var longFileName = this.BuildFileName(itemId.ToString());
            return fileSystemAccessor.IsFileExists(longFileName);
        }

        public bool DeleteItem(Guid itemId)
        {
            var longFileName = this.BuildFileName(itemId.ToString());
            if (fileSystemAccessor.IsFileExists(longFileName))
                fileSystemAccessor.DeleteFile(this.BuildFileName(longFileName));

            return true;
        }


        private string BuildFileName(string fileName)
        {
            return fileSystemAccessor.CombinePath(this._basePath, fileName);
        }

        public string GetPathToBackupFile()
        {
            return this._basePath;
        }

        public void RestoreFromBackupFolder(string path)
        {
            var dirWithCahngelog = fileSystemAccessor.CombinePath(path, CacheFolder);
            
            foreach (var file in fileSystemAccessor.GetFilesInDirectory(this._basePath))
            {
                fileSystemAccessor.DeleteFile(file);
            }

            if (!fileSystemAccessor.IsDirectoryExists(dirWithCahngelog))
                return;

            foreach (var file in fileSystemAccessor.GetFilesInDirectory(dirWithCahngelog))
                fileSystemAccessor.CopyFileOrDirectory(file, fileSystemAccessor.CombinePath(this._basePath, fileSystemAccessor.GetFileName(file)));
        }
    }
}