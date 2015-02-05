using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.Synchronization;

using Moq;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.UnhandledPackageStorageTests
{
    [Subject(typeof(UnhandledPackageStorage))]
    internal class UnhandledPackageStorageTestContext
    {
        protected static UnhandledPackageStorage CreateUnhandledPackageStorage(IFileSystemAccessor fileSystemAccessor=null)
        {
            return new UnhandledPackageStorage(fileSystemAccessor??Mock.Of<IFileSystemAccessor>(), new SyncSettings(appDataDirectory, incomingCapiPackagesWithErrorsDirectoryName, incomingCapiPackageFileNameExtension, incomingCapiPackagesDirectoryName, ""));
        }
        const string appDataDirectory = "App_Data";
        const string incomingCapiPackagesDirectoryName = "IncomingData";
        const string incomingCapiPackagesWithErrorsDirectoryName = "IncomingDataWithErrors";
        const string incomingCapiPackageFileNameExtension = "sync";
    }
}
