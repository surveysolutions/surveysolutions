using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Synchronization;
using WB.Core.Synchronization;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization
{
    internal class UnhandledPackageStorage : IUnhandledPackageStorage
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly string incomingCapiPackagesWithErrorsDirectory;
        private readonly SyncSettings syncSettings;

        public UnhandledPackageStorage(IFileSystemAccessor fileSystemAccessor, SyncSettings syncSettings)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.syncSettings = syncSettings;

            incomingCapiPackagesWithErrorsDirectory = fileSystemAccessor.CombinePath(syncSettings.AppDataDirectory,
               syncSettings.IncomingCapiPackagesWithErrorsDirectoryName);

            if (!fileSystemAccessor.IsDirectoryExists(incomingCapiPackagesWithErrorsDirectory))
                fileSystemAccessor.CreateDirectory(incomingCapiPackagesWithErrorsDirectory);
        }

        public string[] GetListOfUnhandledPackagesForInterview(Guid interviewId)
        {
            var interviewFolder = fileSystemAccessor.CombinePath(incomingCapiPackagesWithErrorsDirectory,
              interviewId.FormatGuid());

            if (!fileSystemAccessor.IsDirectoryExists(interviewFolder))
                return new string[0];

            return fileSystemAccessor.GetFilesInDirectory(interviewFolder);
        }

        public IEnumerable<string> GetListOfUnhandledPackages()
        {
            if (!fileSystemAccessor.IsDirectoryExists(incomingCapiPackagesWithErrorsDirectory))
                return Enumerable.Empty<string>();

            var syncFiles = fileSystemAccessor.GetFilesInDirectory(incomingCapiPackagesWithErrorsDirectory,
                string.Format("*.{0}", syncSettings.IncomingCapiPackageFileNameExtension));

            var result = new List<string>();
            foreach (var syncFile in syncFiles)
            {
                result.Add(fileSystemAccessor.GetFileName(syncFile));
            }

            var interviewFolders = fileSystemAccessor.GetDirectoriesInDirectory(incomingCapiPackagesWithErrorsDirectory);
            foreach (var interviewFolder in interviewFolders)
            {
                var interviewSyncFiles = fileSystemAccessor.GetFilesInDirectory(interviewFolder,
                    string.Format("*.{0}", syncSettings.IncomingCapiPackageFileNameExtension));

                foreach (var interviewSyncFile in interviewSyncFiles)
                {
                    result.Add(fileSystemAccessor.CombinePath(fileSystemAccessor.GetFileName(interviewFolder), fileSystemAccessor.GetFileName(interviewSyncFile)));
                }
            }
            return result;
        }

        public string GetUnhandledPackagePath(string package)
        {
            return fileSystemAccessor.CombinePath(incomingCapiPackagesWithErrorsDirectory, package);
        }

        public void StoreUnhandledPackage(string unhandledPackagePath, Guid? interviewId)
        {
            if (interviewId.HasValue)
                StoreErrorPackageAtInterviewCorrespondingFolder(interviewId.Value, unhandledPackagePath);
            else
                fileSystemAccessor.CopyFileOrDirectory(unhandledPackagePath, incomingCapiPackagesWithErrorsDirectory);
        }

        private void StoreErrorPackageAtInterviewCorrespondingFolder(Guid interviewId, string fileToProcess)
        {
            var interviewFolder = fileSystemAccessor.CombinePath(incomingCapiPackagesWithErrorsDirectory,
               interviewId.FormatGuid());

            if (!fileSystemAccessor.IsDirectoryExists(interviewFolder))
                fileSystemAccessor.CreateDirectory(interviewFolder);

            fileSystemAccessor.CopyFileOrDirectory(fileToProcess, interviewFolder);
        }
    }
}
