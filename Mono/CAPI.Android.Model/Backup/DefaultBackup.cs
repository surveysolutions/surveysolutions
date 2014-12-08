
using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.ErrorReporting;
using WB.Core.Infrastructure.Backup;
using WB.Core.Infrastructure.FileSystem;
using Environment = Android.OS.Environment;

namespace CAPI.Android.Core.Model.Backup
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
            get { return restorePath; }
        }
        private readonly IEnumerable<IBackupable> backupables;

        public DefaultBackup(string basePath, IInfoFileSupplierRegistry infoFileSupplierRegistry, IArchiveUtils archiveUtils,
            IFileSystemAccessor fileSystemAccessor, params IBackupable[] backupables)
        {
            this.archiveUtils = archiveUtils;
            this.fileSystemAccessor = fileSystemAccessor;
            this.backupables = backupables;

            rootPath = fileSystemAccessor.CombinePath(basePath, Capi);
            if (!fileSystemAccessor.IsDirectoryExists(rootPath))
            {
                fileSystemAccessor.CreateDirectory(rootPath);
            }

            backupPath = fileSystemAccessor.CombinePath(rootPath, BackupFolder);
            if (!fileSystemAccessor.IsDirectoryExists(backupPath))
                fileSystemAccessor.CreateDirectory(backupPath);
            restorePath = fileSystemAccessor.CombinePath(rootPath, RestoreFolder);
            if (!fileSystemAccessor.IsDirectoryExists(restorePath))
                fileSystemAccessor.CreateDirectory(restorePath);

            infoFileSupplierRegistry.Register(this.Backup);
        }

        public string Backup()
        {
            var backupFolderName = string.Format("backup-{0}", DateTime.Now.Ticks);
            var backupFolderPath = fileSystemAccessor.CombinePath(backupPath, backupFolderName);
            fileSystemAccessor.CreateDirectory(backupFolderPath);

            foreach (var backupable in backupables)
            {
                var path = backupable.GetPathToBackupFile();
                
                if ((!string.IsNullOrEmpty(path)) && (fileSystemAccessor.IsFileExists(path) || fileSystemAccessor.IsDirectoryExists(path)))
                    fileSystemAccessor.CopyFileOrDirectory(path, backupFolderPath);
            }
            var backupArchiveName = fileSystemAccessor.CombinePath(backupPath, backupFolderName + zipExtension);
            archiveUtils.ZipDirectory(backupFolderPath, backupArchiveName);
            fileSystemAccessor.DeleteDirectory(backupFolderPath);
            return backupArchiveName;
        }

        public void Restore()
        {
            var files = fileSystemAccessor.GetFilesInDirectory(restorePath).Where(fileName => fileName.EndsWith(zipExtension)).ToArray();
            if (files.Length == 0)
                throw new ArgumentException("Restore archive is absent");

            var firstFile = files[0];
            var unziperFolder = fileSystemAccessor.CombinePath(restorePath, GetArchiveName(firstFile));
            archiveUtils.Unzip(firstFile, restorePath);
            foreach (var backupable in backupables)
            {
                backupable.RestoreFromBackupFolder(unziperFolder);
            }
            fileSystemAccessor.DeleteDirectory(unziperFolder);
        }

        private string GetArchiveName(string pathToFile)
        {
            var fileNameWithExtension = fileSystemAccessor.GetFileName(pathToFile);
            return fileNameWithExtension.Replace(zipExtension, "");
        }
    }
}