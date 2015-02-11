using System;
using System.Collections.Generic;
using System.IO;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Tests.Unit.SharedKernels.SurveyManagement.SyncPackagesProcessorTests;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.UnhandledPackageStorageTests
{
    internal class when_GetListOfUnhandledPackages_is_called_and_error_folder_is_present_and_not_empty : UnhandledPackageStorageTestContext
    {
        Establish context = () =>
        {
            fileSystemAccessorMock = new Mock<IFileSystemAccessor>();

            fileSystemAccessorMock.Setup(x => x.IsDirectoryExists(Moq.It.IsAny<string>()))
              .Returns(true);

            fileSystemAccessorMock.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Returns<string, string>(Path.Combine);

            fileSystemAccessorMock.Setup(x => x.GetFileName(Moq.It.IsAny<string>()))
               .Returns<string>(Path.GetFileName);

            fileSystemAccessorMock.Setup(x => x.GetFilesInDirectory(@"App_Data\IncomingDataWithErrors", Moq.It.IsAny<string>()))
                .Returns(filesWithoutInterview);

            fileSystemAccessorMock.Setup(x => x.GetDirectoriesInDirectory(@"App_Data\IncomingDataWithErrors"))
                .Returns(new[] {interviewId.FormatGuid()});


            fileSystemAccessorMock.Setup(x => x.GetFilesInDirectory(interviewId.FormatGuid(), Moq.It.IsAny<string>()))
                .Returns(filesWithInterview);

            brokenSyncPackagesStorage = CreateUnhandledPackageStorage(fileSystemAccessor: fileSystemAccessorMock.Object);
        };

        Because of = () =>
            result = brokenSyncPackagesStorage.GetListOfUnhandledPackages();

        It should_result_not_empty = () =>
           result.ShouldEqual(new[] { "f1.sync", "f2.sync", interviewId.FormatGuid() + @"\" + "i1.sync", interviewId.FormatGuid() + @"\" + "i2.sync" });

        private static BrokenSyncPackagesStorage brokenSyncPackagesStorage;
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock;
        private static IEnumerable<string> result;
        private static string[] filesWithoutInterview = new[] {"f1.sync", "f2.sync"};

        private static string[] filesWithInterview = new[] { "i1.sync", "i2.sync" };
        private static Guid interviewId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
    }
}
