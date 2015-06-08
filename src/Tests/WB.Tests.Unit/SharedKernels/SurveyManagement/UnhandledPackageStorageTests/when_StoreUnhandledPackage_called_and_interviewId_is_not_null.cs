using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.UnhandledPackageStorageTests
{
    internal class when_StoreUnhandledPackage_called_and_interviewId_is_not_null : UnhandledPackageStorageTestContext
    {
        Establish context = () =>
        {
            fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Returns<string, string>(Path.Combine);

            fileSystemAccessorMock.Setup(x => x.GetFileNameWithoutExtension(Moq.It.IsAny<string>()))
              .Returns(interviewId.FormatGuid());

            brokenSyncPackagesStorage = CreateUnhandledPackageStorage(fileSystemAccessor: fileSystemAccessorMock.Object);
        };

        Because of = () =>
            brokenSyncPackagesStorage.StoreUnhandledPackageForInterview("test", interviewId, nullReferenceException);

        It should_copy_package_to_error_folder = () =>
           fileSystemAccessorMock.Verify(x => x.CopyFileOrDirectory("test", @"App_Data\IncomingDataWithErrors\categorized\unknown\" + interviewId.FormatGuid()), Times.Once);

        It should_create_exception_file_with_exception_message = () =>
            fileSystemAccessorMock.Verify(x => x.WriteAllText(string.Format(@"App_Data\IncomingDataWithErrors\categorized\unknown\{0}\{0}.exception", interviewId.FormatGuid()), nullReferenceException.Message + " "), Times.Once);

        private static BrokenSyncPackagesStorage brokenSyncPackagesStorage;
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock;
        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static NullReferenceException nullReferenceException = new NullReferenceException("test message");
    }
}
