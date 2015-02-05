using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.UnhandledPackageStorageTests
{
    internal class when_StoreUnhandledPackage_called_and_interviewId_is_null : UnhandledPackageStorageTestContext
    {
        Establish context = () =>
        {
            fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Returns<string, string>(Path.Combine);

            unhandledPackageStorage = CreateUnhandledPackageStorage(fileSystemAccessor: fileSystemAccessorMock.Object);
        };

        Because of = () =>
            unhandledPackageStorage.StoreUnhandledPackage("test",null);

        It should_copy_package_to_error_folder = () =>
           fileSystemAccessorMock.Verify(x => x.CopyFileOrDirectory("test",@"App_Data\IncomingDataWithErrors"), Times.Once);

        private static UnhandledPackageStorage unhandledPackageStorage;
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock;
    }
}
