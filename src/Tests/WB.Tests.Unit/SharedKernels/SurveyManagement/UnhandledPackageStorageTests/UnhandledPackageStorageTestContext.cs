using Machine.Specifications;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.Synchronization;

using Moq;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.UnhandledPackageStorageTests
{
    [Subject(typeof(BrokenSyncPackagesStorage))]
    internal class UnhandledPackageStorageTestContext
    {
        protected static BrokenSyncPackagesStorage CreateUnhandledPackageStorage(IFileSystemAccessor fileSystemAccessor=null)
        {
            return new BrokenSyncPackagesStorage(fileSystemAccessor??Mock.Of<IFileSystemAccessor>(), new SyncSettings(AppDataDirectory, IncomingCapiPackagesWithErrorsDirectoryName, IncomingCapiPackageFileNameExtension, IncomingCapiPackagesDirectoryName, ""));
        }
        const string AppDataDirectory = "App_Data";
        const string IncomingCapiPackagesDirectoryName = "IncomingData";
        const string IncomingCapiPackagesWithErrorsDirectoryName = "IncomingDataWithErrors";
        const string IncomingCapiPackageFileNameExtension = "sync";
    }
}
