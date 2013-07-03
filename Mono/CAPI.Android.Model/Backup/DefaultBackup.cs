
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
        private readonly string backupPath;
        private readonly IEnumerable<IBackupable> backupables;
        public DefaultBackup(params IBackupable[] backupables)
        {
            this.backupables = backupables;
            backupPath = Environment.ExternalStorageDirectory.AbsolutePath;
            if (Directory.Exists(backupPath))
            {
                backupPath = System.IO.Path.Combine(backupPath, Capi);
                if (!Directory.Exists(backupPath))
                {
                    Directory.CreateDirectory(backupPath);
                }
                backupPath = System.IO.Path.Combine(backupPath, BackupFolder);
                if (!Directory.Exists(backupPath))
                    Directory.CreateDirectory(backupPath);

            }
            else
            {
                backupPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            }
        }

        public string Backup()
        {
            var backupFolderName = string.Format("backup-{0}", DateTime.Now.Ticks);
            var backupFolderPath = Path.Combine(backupPath, backupFolderName);
            Directory.CreateDirectory(backupFolderPath);

            foreach (var backupable in backupables)
            {
                CopyFileOrDirectory(backupable.GetPathToBakupFile(), backupFolderPath);
            }
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

        public void Restore(string path)
        {
            if (!Directory.Exists(path))
                throw new ArgumentException("Retore Directory is absent");
            foreach (var backupable in backupables)
            {
                backupable.RestoreFromBakupFolder(path);
            }
        }
    }
}