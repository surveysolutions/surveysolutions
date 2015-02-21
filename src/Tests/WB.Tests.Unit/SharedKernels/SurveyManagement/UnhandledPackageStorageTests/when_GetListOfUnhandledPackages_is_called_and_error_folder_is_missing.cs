using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Tests.Unit.SharedKernels.SurveyManagement.SyncPackagesProcessorTests;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.UnhandledPackageStorageTests
{
    internal class when_GetListOfUnhandledPackages_is_called_and_error_folder_is_missing : UnhandledPackageStorageTestContext
    {
        Establish context = () =>
        {
            fileSystemAccessorMock = new Mock<IFileSystemAccessor>();

            brokenSyncPackagesStorage = CreateUnhandledPackageStorage(fileSystemAccessor: fileSystemAccessorMock.Object);
        };

        Because of = () =>
            result = brokenSyncPackagesStorage.GetListOfUnhandledPackages();

        It should_result_be_empty = () =>
           result.ShouldBeEmpty();

        private static BrokenSyncPackagesStorage brokenSyncPackagesStorage;
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock;
        private static IEnumerable<string> result;
    }
}
