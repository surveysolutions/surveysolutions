using System;
using System.Collections.Generic;
using System.IO;
using CAPI.Android.Core.Model.Backup;
using WB.Core.Infrastructure.Backup;

namespace CapiDataGenerator
{
    public class DefaultBackup : IBackup
    {
        private const string Capi = "CAPI";
        private const string BackupFolder = "Backup";
        private const string ImagesFolderName = "IMAGES";
        private readonly string backupPath;
        private readonly string rootPath;
        public string RestorePath
        {
            get { return string.Empty; }
        }
        private readonly IEnumerable<IBackupable> backupables;

        public DefaultBackup(params IBackupable[] backupables)
        {
            this.backupables = backupables;

            rootPath = Directory.Exists(Environment.CurrentDirectory)
                             ? Environment.CurrentDirectory
                             : System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

            rootPath = System.IO.Path.Combine(rootPath, Capi);
            if (!Directory.Exists(rootPath))
            {
                Directory.CreateDirectory(rootPath);
            }
            backupPath = System.IO.Path.Combine(rootPath, BackupFolder);
            if (!Directory.Exists(backupPath))
                Directory.CreateDirectory(backupPath);

        }

        public string Backup()
        {
            var backupFolderName = string.Format("backup-{0}", DateTime.Now.Ticks);
            var backupFolderPath = Path.Combine(backupPath, backupFolderName);
            
            Directory.CreateDirectory(backupFolderPath);
            Directory.CreateDirectory(Path.Combine(backupFolderPath, ImagesFolderName));

            foreach (var backupable in backupables)
            {
                var path = backupable.GetPathToBakupFile();
                if (string.IsNullOrEmpty(path))
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
            if (sourceFileName == null)
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
        }
    }
}