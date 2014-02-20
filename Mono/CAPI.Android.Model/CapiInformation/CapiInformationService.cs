using System;
using System.Collections.Generic;
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
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.InformationSupplier;
using Environment = Android.OS.Environment;

namespace CAPI.Android.Core.Model.CapiInformation
{
    internal class CapiInformationService : ICapiInformationService
    {
        private readonly string infoPackagesPath;
        private readonly string basePath;
        private readonly IInfoFileSupplierRegistry infoFileSupplierRegistry;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IArchiveUtils archiveUtils;
        private const string InfoPackageFolderName = "InfoPackage";

        public CapiInformationService(IInfoFileSupplierRegistry infoFileSupplierRegistry, IFileSystemAccessor fileSystemAccessor,
            IArchiveUtils archiveUtils, string basePath)
        {
            this.infoFileSupplierRegistry = infoFileSupplierRegistry;
            this.fileSystemAccessor = fileSystemAccessor;
            this.archiveUtils = archiveUtils;
            this.basePath = basePath;
            this.infoPackagesPath = GetOrCreateInfoPackageFolderName();
        }

        public string CreateInformationPackage()
        {
            var infoPackageFolderName = CreateNewInfoPackageFolderName();
            var infoPackageFolderPath = fileSystemAccessor.CombinePath(infoPackagesPath, infoPackageFolderName);
            fileSystemAccessor.CreateDirectory(infoPackageFolderPath);

            foreach (var infoFilePath in infoFileSupplierRegistry.GetAll())
            {
                fileSystemAccessor.CopyFileOrDirectory(infoFilePath, infoPackageFolderPath);
            }

            var infoPackageFilePath = fileSystemAccessor.CombinePath(infoPackagesPath, infoPackageFolderName + ".zip");

            archiveUtils.ZipDirectory(infoPackageFolderPath, infoPackageFilePath);

            fileSystemAccessor.DeleteDirectory(infoPackageFolderPath);
            return infoPackageFilePath;
        }

        private string CreateNewInfoPackageFolderName()
        {
            return string.Format("info-package-{0}", DateTime.Now.Ticks);
        }

        private string GetOrCreateInfoPackageFolderName()
        {
            var result = fileSystemAccessor.CombinePath(this.basePath, InfoPackageFolderName);
            if (!fileSystemAccessor.IsDirectoryExists(result))
            {
                fileSystemAccessor.IsDirectoryExists(result);
            }
            return result;
        }
    }
}