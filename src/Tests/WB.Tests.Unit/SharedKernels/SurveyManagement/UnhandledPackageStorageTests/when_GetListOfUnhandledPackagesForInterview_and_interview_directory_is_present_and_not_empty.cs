using System;
using System.IO;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.UnhandledPackageStorageTests
{
    internal class when_GetListOfUnhandledPackagesForInterview_and_interview_directory_is_present_and_not_empty : UnhandledPackageStorageTestContext
    {
        Establish context = () =>
        {
            fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Returns<string, string>(Path.Combine);

            fileSystemAccessorMock.Setup(x => x.IsDirectoryExists(Moq.It.IsAny<string>()))
             .Returns(true);

            fileSystemAccessorMock.Setup(x => x.GetFilesInDirectory(Moq.It.IsAny<string>()))
                .Returns(filesInFolder);

            brokenSyncPackagesStorage = CreateUnhandledPackageStorage(fileSystemAccessor: fileSystemAccessorMock.Object);
        };

        Because of = () =>
            result = brokenSyncPackagesStorage.GetListOfUnhandledPackagesForInterview(interviewId);

        It should_result_return_all_files_in_folder = () =>
           result.ShouldEqual(filesInFolder);

        private static BrokenSyncPackagesStorage brokenSyncPackagesStorage;
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock;
        private static Guid interviewId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static string[] result;
        private static string[] filesInFolder = new[] { "f1", "f2" };
    }
}
