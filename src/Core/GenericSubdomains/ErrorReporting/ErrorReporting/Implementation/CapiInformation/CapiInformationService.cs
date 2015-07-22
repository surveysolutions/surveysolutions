using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.ErrorReporting.Services.CapiInformationService;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Core.GenericSubdomains.ErrorReporting.Implementation.CapiInformation
{
    internal class CapiInformationService : ICapiInformationService
    {
        private readonly string infoPackagesPath;
        private readonly string basePath;
        private readonly IInfoFileSupplierRegistry infoFileSupplierRegistry;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IArchiveUtils archiveUtils;
        private const string InfoPackageFolderName = "InfoPackage";

        public CapiInformationService(
            IInfoFileSupplierRegistry infoFileSupplierRegistry, 
            IFileSystemAccessor fileSystemAccessor,
            IArchiveUtils archiveUtils, string basePath)
        {
            this.infoFileSupplierRegistry = infoFileSupplierRegistry;
            this.fileSystemAccessor = fileSystemAccessor;
            this.archiveUtils = archiveUtils;
            this.basePath = basePath;
            this.infoPackagesPath = this.GetOrCreateInfoPackageFolderName();
        }

        public Task<string> CreateInformationPackage(CancellationToken ct)
        {
            return Task.Factory.StartNew(() =>
            {
                var infoPackageFolderName = this.CreateNewInfoPackageFolderName();
                var infoPackageFolderPath = this.fileSystemAccessor.CombinePath(this.infoPackagesPath,
                    infoPackageFolderName);

                ExitIfCanceled(ct);
                this.fileSystemAccessor.CreateDirectory(infoPackageFolderPath);

                foreach (var infoFilePath in this.infoFileSupplierRegistry.GetAll())
                {
                    ExitIfCanceled(ct);
                    if (fileSystemAccessor.IsFileExists(infoFilePath))
                        this.fileSystemAccessor.CopyFileOrDirectory(infoFilePath, infoPackageFolderPath);
                }

                ExitIfCanceled(ct);

                var infoPackageFilePath = this.fileSystemAccessor.CombinePath(this.infoPackagesPath,
                    infoPackageFolderName + ".zip");

                this.archiveUtils.ZipDirectory(infoPackageFolderPath, infoPackageFilePath);
                this.fileSystemAccessor.DeleteDirectory(infoPackageFolderPath);

                return infoPackageFilePath;
            }, ct);
        }

        private string CreateNewInfoPackageFolderName()
        {
            return string.Format("info-package-{0}", DateTime.Now.Ticks);
        }

        private string GetOrCreateInfoPackageFolderName()
        {
            var result = this.fileSystemAccessor.CombinePath(this.basePath, InfoPackageFolderName);
            if (!this.fileSystemAccessor.IsDirectoryExists(result))
            {
                this.fileSystemAccessor.IsDirectoryExists(result);
            }
            return result;
        }

        private void ExitIfCanceled(CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                ct.ThrowIfCancellationRequested();
        }
    }
}