using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using WB.Core.Infrastructure.Backup;

namespace CAPI.Android.Core.Model.Backup
{
    public class FolderBackupable:IBackupable
    {
        private readonly string basePath;
        private readonly string folderName;

        public FolderBackupable(string basePath, string folderName)
        {
            this.basePath = basePath;
            this.folderName = folderName;
        }

        public string GetPathToBackupFile()
        {
            var pathToFolder = Path.Combine(basePath, folderName);
            if (Directory.Exists(pathToFolder))
                return pathToFolder;

            return string.Empty;
        }

        public void RestoreFromBackupFolder(string path)
        {
            var pathToBackupedFolder = Path.Combine(path, folderName);

            var fullPath = this.GetPathToBackupFile();

            if (Directory.Exists(pathToBackupedFolder))
            {
                if(Directory.Exists(fullPath))
                    Directory.Delete(fullPath, true);

                CopyFileOrDirectory(pathToBackupedFolder, basePath);
            }
            else
                Directory.Delete(fullPath, true);
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
                    CopyFileOrDirectory(directory, destDir);
            }
            else
            {
                this.CopyFile(sourceDir, targetDir);
            }
        }
        private void CopyFile(string sourcePath, string backupFolderPath)
        {
            var sourceFileName = Path.GetFileName(sourcePath);
            if (sourceFileName == null)
                return;
            File.Copy(sourcePath, Path.Combine(backupFolderPath, sourceFileName), true);
        }
    }
}