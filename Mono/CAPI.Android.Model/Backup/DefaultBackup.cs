
using System;
using System.Collections.Generic;
using System.IO;
using WB.Core.Infrastructure.Backup;
using Environment = Android.OS.Environment;

namespace CAPI.Android.Core.Model.Backup
{
    public class DefaultBackup : IBackup
    {
        private const string Capi = "CAPI";
        private const string BackupFolder = "Backup";
        private const string RestoreFolder = "Restore";

        private readonly string backupPath;
        private readonly string restorePath;
        private readonly string rootPath;
        public string RestorePath {
            get { return restorePath; }
        }
        private readonly IEnumerable<IBackupable> backupables;

        public DefaultBackup(params IBackupable[] backupables)
        {
            this.backupables = backupables;

            rootPath = Directory.Exists(Environment.ExternalStorageDirectory.AbsolutePath)
                             ? Environment.ExternalStorageDirectory.AbsolutePath
                             : System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

            rootPath = System.IO.Path.Combine(rootPath, Capi);
            if (!Directory.Exists(rootPath))
            {
                Directory.CreateDirectory(rootPath);
            }
            backupPath = System.IO.Path.Combine(rootPath, BackupFolder);
            if (!Directory.Exists(backupPath))
                Directory.CreateDirectory(backupPath);
            restorePath = System.IO.Path.Combine(rootPath, RestoreFolder);
            if (!Directory.Exists(restorePath))
                Directory.CreateDirectory(restorePath);

        }

        public string Backup()
        {
            var backupFolderName = string.Format("backup-{0}", DateTime.Now.Ticks);
            var backupFolderPath = Path.Combine(backupPath, backupFolderName);
            Directory.CreateDirectory(backupFolderPath);

            foreach (var backupable in backupables)
            {
                var path = backupable.GetPathToBakupFile();
                if(string.IsNullOrEmpty(path))
                    continue;
                
                CopyFileOrDirectory(path, backupFolderPath);
            }
            AndroidZipUtility.ZipDirectory(backupFolderPath, Path.Combine(backupPath, backupFolderName + ".zip"));
            Directory.Delete(backupFolderPath, true);
            return backupFolderPath;
        }

        private void CopyDb(string sourcePath, string backupFolderPath)
        {
            var sourceFileName = Path.GetFileName(sourcePath);
            if(sourceFileName==null)
                return;
            File.Copy(sourcePath, Path.Combine(backupFolderPath, sourceFileName), true);
        }

        private void CopyFileOrDirectory(string sourceDir, string targetDir)
        {
            FileAttributes attr = File.GetAttributes(sourceDir);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                var sourceDirectoryName = Path.GetFileName(sourceDir);
                if (sourceDirectoryName == null)
                    return;
                var destDir = Path.Combine(targetDir, sourceDirectoryName);
                Directory.CreateDirectory(destDir);
                foreach (var file in Directory.GetFiles(sourceDir))
                    File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)));

                foreach (var directory in Directory.GetDirectories(sourceDir))
                    CopyFileOrDirectory(directory, Path.Combine(destDir, sourceDirectoryName));
            }
            else
            {
                CopyDb(sourceDir, targetDir);
            }
        }

        public void Restore()
        {
            var files = Directory.GetFiles(restorePath);
            if(files.Length==0)
                throw new ArgumentException("Restore archive is absent");

            var firstFile = files[0];
            var unziperFolder = Path.Combine(restorePath, Path.GetFileNameWithoutExtension(firstFile));
            AndroidZipUtility.Unzip(firstFile, restorePath);
            foreach (var backupable in backupables)
            {
                backupable.RestoreFromBakupFolder(unziperFolder);
            }
            Directory.Delete(unziperFolder, true);
        }
    }
}