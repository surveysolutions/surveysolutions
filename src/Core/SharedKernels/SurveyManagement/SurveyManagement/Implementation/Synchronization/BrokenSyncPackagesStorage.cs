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
    internal class BrokenSyncPackagesStorage : IBrokenSyncPackagesStorage
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly string incomingCapiPackagesWithErrorsDirectory;
        private readonly SyncSettings syncSettings;

        private readonly string exceptionExtension = ".exception";
        private readonly string unknownFolderName = "unknown";
        private readonly string categorizedFolderName = "categorized";

        public BrokenSyncPackagesStorage(IFileSystemAccessor fileSystemAccessor, SyncSettings syncSettings)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.syncSettings = syncSettings;

            incomingCapiPackagesWithErrorsDirectory = fileSystemAccessor.CombinePath(syncSettings.AppDataDirectory,
               syncSettings.IncomingCapiPackagesWithErrorsDirectoryName);

            if (!fileSystemAccessor.IsDirectoryExists(incomingCapiPackagesWithErrorsDirectory))
                fileSystemAccessor.CreateDirectory(incomingCapiPackagesWithErrorsDirectory);
        }

        public IEnumerable<string> GetListOfUnhandledPackages()
        {
            if (!fileSystemAccessor.IsDirectoryExists(incomingCapiPackagesWithErrorsDirectory))
                return Enumerable.Empty<string>();

            return GetListOfUnhandledPackagesInDirectoryRecursively(incomingCapiPackagesWithErrorsDirectory);
        }

        private List<string> GetListOfUnhandledPackagesInDirectoryRecursively(string rootDirectory)
        {
            var result = new List<string>();

            foreach (string nestedDirectory in fileSystemAccessor.GetDirectoriesInDirectory(rootDirectory))
            {
                var interviewSyncFiles = fileSystemAccessor.GetFilesInDirectory(nestedDirectory,
                    string.Format("*.{0}", syncSettings.IncomingCapiPackageFileNameExtension));

                result.AddRange(interviewSyncFiles);

                result.AddRange(GetListOfUnhandledPackagesInDirectoryRecursively(nestedDirectory));
            }

            return result;
        }

        public string GetUnhandledPackagePath(string package)
        {
            return fileSystemAccessor.CombinePath(incomingCapiPackagesWithErrorsDirectory, package);
        }

        private void StoreUnhandledPackage(string unhandledPackagePath, string folderToStore, Exception e)
        {
            fileSystemAccessor.CopyFileOrDirectory(unhandledPackagePath, folderToStore);

            var fileName = fileSystemAccessor.GetFileNameWithoutExtension(unhandledPackagePath) + exceptionExtension;

            fileSystemAccessor.WriteAllText(fileSystemAccessor.CombinePath(folderToStore, fileName), CreateContentOfExceptionFile(e));
        }

        public void StoreUnknownUnhandledPackage(string unhandledPackagePath, Exception e)
        {
            var folderToStore = fileSystemAccessor.CombinePath(incomingCapiPackagesWithErrorsDirectory,
                unknownFolderName);
            
            if (!fileSystemAccessor.IsDirectoryExists(folderToStore))
                fileSystemAccessor.CreateDirectory(folderToStore);
            
            StoreUnhandledPackage(unhandledPackagePath, folderToStore, e);
        }

        public void StoreUnhandledPackageForInterview(string unhandledPackagePath, Guid interviewId, Exception e)
        {
            StoreUnhandledPackageForInterviewInTypedFolder(unhandledPackagePath, interviewId, e, unknownFolderName);
        }

        public void StoreUnhandledPackageForInterviewInTypedFolder(string unhandledPackagePath, Guid interviewId, Exception e,
            string typeFolderName)
        {
            var folderToStoreCategorizedPackages = fileSystemAccessor.CombinePath(incomingCapiPackagesWithErrorsDirectory, categorizedFolderName);

            if (!fileSystemAccessor.IsDirectoryExists(folderToStoreCategorizedPackages))
                fileSystemAccessor.CreateDirectory(folderToStoreCategorizedPackages);

            var folderToStorePackagesWithErrorType = fileSystemAccessor.CombinePath(folderToStoreCategorizedPackages, typeFolderName);

            if (!fileSystemAccessor.IsDirectoryExists(folderToStorePackagesWithErrorType))
                fileSystemAccessor.CreateDirectory(folderToStorePackagesWithErrorType);

            var folderToStorePackagesForInterview= fileSystemAccessor.CombinePath(folderToStorePackagesWithErrorType,
                      interviewId.FormatGuid());

            if (!fileSystemAccessor.IsDirectoryExists(folderToStorePackagesForInterview))
                fileSystemAccessor.CreateDirectory(folderToStorePackagesForInterview);

            StoreUnhandledPackage(unhandledPackagePath, folderToStorePackagesForInterview, e);
        }

        private string CreateContentOfExceptionFile(Exception e)
        {
            return string.Join(Environment.NewLine, e.UnwrapAllInnerExceptions().Select(ex =>
                string.Format("{0} {1}",
                    ex.Message, ex.StackTrace)));
        }
    }
}
