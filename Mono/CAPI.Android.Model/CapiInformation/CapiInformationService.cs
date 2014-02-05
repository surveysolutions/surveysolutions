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
using CAPI.Android.Core.Model.Backup;
using WB.Core.Infrastructure.Backup;
using WB.Core.Infrastructure.InformationSupplier;
using Environment = Android.OS.Environment;

namespace CAPI.Android.Core.Model.CapiInformation
{
    internal class CapiInformationService : ICapiInformationService
    {
        private readonly string infoPackagesPath;
        private readonly IInfoFileSupplierRegistry infoFileSupplierRegistry;
        private const string InfoPackageFolderName = "InfoPackage";

        public CapiInformationService(IInfoFileSupplierRegistry infoFileSupplierRegistry)
        {
            this.infoFileSupplierRegistry = infoFileSupplierRegistry;
            this.infoPackagesPath = GetOrCreateInfoPackageFolderName();
        }

        public string CreateInformationPackage()
        {
            var infoPackageFolderName = CreateNewInfoPackageFolderName();
            var infoPackageFolderPath = Path.Combine(infoPackagesPath, infoPackageFolderName);
            Directory.CreateDirectory(infoPackageFolderPath);

            foreach (var infoFilePath in infoFileSupplierRegistry.GetAll())
            {
                CopyFileOrDirectory(infoFilePath, infoPackageFolderPath);
            }

            AndroidZipUtility.ZipDirectory(infoPackageFolderPath, Path.Combine(infoPackagesPath, infoPackageFolderName + ".zip"));
            Directory.Delete(infoPackageFolderPath, true);
            return infoPackageFolderPath;
        }

        private string CreateNewInfoPackageFolderName()
        {
            return string.Format("info-package-{0}", DateTime.Now.Ticks);
        }

        private string GetOrCreateInfoPackageFolderName()
        {
            var rootPath = Directory.Exists(Environment.ExternalStorageDirectory.AbsolutePath)
                            ? Environment.ExternalStorageDirectory.AbsolutePath
                            : System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

            var result = System.IO.Path.Combine(rootPath, InfoPackageFolderName);
            if (!Directory.Exists(result))
            {
                Directory.CreateDirectory(result);
            }
            return result;
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
    }
}