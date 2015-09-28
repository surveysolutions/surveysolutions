using System;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.Infrastructure.Backup;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
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
            this.fileSystemAccessor.WriteAllText(this.BuildFileName(itemId.ToString()), itemContent);
            return true;
        }

        public string LoadItem(Guid itemId)
        {
            var longFileName = this.BuildFileName(itemId.ToString());
            if (this.fileSystemAccessor.IsFileExists(longFileName))
                return this.fileSystemAccessor.ReadAllText(longFileName);
            return null;
        }

        public bool DoesCachedItemExist(Guid itemId)
        {
            var longFileName = this.BuildFileName(itemId.ToString());
            return this.fileSystemAccessor.IsFileExists(longFileName);
        }

        public bool DeleteItem(Guid itemId)
        {
            var longFileName = this.BuildFileName(itemId.ToString());
            if (this.fileSystemAccessor.IsFileExists(longFileName))
                this.fileSystemAccessor.DeleteFile(this.BuildFileName(longFileName));

            return true;
        }


        private string BuildFileName(string fileName)
        {
            return this.fileSystemAccessor.CombinePath(this._basePath, fileName);
        }

        public string GetPathToBackupFile()
        {
            return this._basePath;
        }

        public void RestoreFromBackupFolder(string path)
        {
            var dirWithCahngelog = this.fileSystemAccessor.CombinePath(path, CacheFolder);
            
            foreach (var file in this.fileSystemAccessor.GetFilesInDirectory(this._basePath))
            {
                this.fileSystemAccessor.DeleteFile(file);
            }

            if (!this.fileSystemAccessor.IsDirectoryExists(dirWithCahngelog))
                return;

            foreach (var file in this.fileSystemAccessor.GetFilesInDirectory(dirWithCahngelog))
                this.fileSystemAccessor.CopyFileOrDirectory(file, this._basePath);
        }
    }
}