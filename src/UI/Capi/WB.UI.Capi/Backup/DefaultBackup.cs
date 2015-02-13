using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.Backup;
using WB.Core.Infrastructure.FileSystem;

namespace WB.UI.Capi.Backup
{
    public class DefaultBackup : IBackup
    {
        private const string Capi = "CAPI";
        private const string BackupFolder = "Backup";
        private const string RestoreFolder = "Restore";
        private const string zipExtension = ".zip";
        private readonly IArchiveUtils archiveUtils;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly string backupPath;
        private readonly string restorePath;
        private readonly string rootPath;
        public string RestorePath {
            get { return this.restorePath; }
        }
        private readonly IEnumerable<IBackupable> backupables;

        public DefaultBackup(string basePath, IInfoFileSupplierRegistry infoFileSupplierRegistry, IArchiveUtils archiveUtils,
            IFileSystemAccessor fileSystemAccessor, params IBackupable[] backupables)
        {
            this.archiveUtils = archiveUtils;
            this.fileSystemAccessor = fileSystemAccessor;
            this.backupables = backupables;

            this.rootPath = fileSystemAccessor.CombinePath(basePath, Capi);
            if (!fileSystemAccessor.IsDirectoryExists(this.rootPath))
            {
                fileSystemAccessor.CreateDirectory(this.rootPath);
            }

            this.backupPath = fileSystemAccessor.CombinePath(this.rootPath, BackupFolder);
            if (!fileSystemAccessor.IsDirectoryExists(this.backupPath))
                fileSystemAccessor.CreateDirectory(this.backupPath);
            this.restorePath = fileSystemAccessor.CombinePath(this.rootPath, RestoreFolder);
            if (!fileSystemAccessor.IsDirectoryExists(this.restorePath))
                fileSystemAccessor.CreateDirectory(this.restorePath);

            infoFileSupplierRegistry.Register(this.Backup);
        }

        public string Backup()
        {
            var backupFolderName = string.Format("backup-{0}", DateTime.Now.Ticks);
            var backupFolderPath = this.fileSystemAccessor.CombinePath(this.backupPath, backupFolderName);
            this.fileSystemAccessor.CreateDirectory(backupFolderPath);

            foreach (var backupable in this.backupables)
            {
                var path = backupable.GetPathToBackupFile();
                
                if ((!string.IsNullOrEmpty(path)) && (this.fileSystemAccessor.IsFileExists(path) || this.fileSystemAccessor.IsDirectoryExists(path)))
                    this.fileSystemAccessor.CopyFileOrDirectory(path, backupFolderPath);
            }
            var backupArchiveName = this.fileSystemAccessor.CombinePath(this.backupPath, backupFolderName + zipExtension);
            this.archiveUtils.ZipDirectory(backupFolderPath, backupArchiveName);
            this.fileSystemAccessor.DeleteDirectory(backupFolderPath);
            return backupArchiveName;
        }

        public void Restore()
        {
            var files = this.fileSystemAccessor.GetFilesInDirectory(this.restorePath).Where(fileName => fileName.EndsWith(zipExtension)).ToArray();
            if (files.Length == 0)
                throw new ArgumentException("Restore archive is absent");

            var firstFile = files[0];
            var unziperFolder = this.fileSystemAccessor.CombinePath(this.restorePath, this.GetArchiveName(firstFile));
            this.archiveUtils.Unzip(firstFile, this.restorePath);
            foreach (var backupable in this.backupables)
            {
                backupable.RestoreFromBackupFolder(unziperFolder);
            }
            this.fileSystemAccessor.DeleteDirectory(unziperFolder);
        }

        private string GetArchiveName(string pathToFile)
        {
            var fileNameWithExtension = this.fileSystemAccessor.GetFileName(pathToFile);
            return fileNameWithExtension.Replace(zipExtension, "");
        }
    }
}