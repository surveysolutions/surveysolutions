using System;
using System.IO;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Synchronization;
using WB.Core.Synchronization;
using WB.Tests.Unit.SharedKernels.SurveyManagement.SyncPackagesProcessorTests;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.IncomingPackagesQueueTests
{
    internal class when_DeQueue_called_and_package_cant_be_opened : IncomingPackagesQueueTestContext
    {
        Establish context = () =>
        {
            var jsonUtilsMock = new Mock<IJsonUtils>();
            jsonUtilsMock.Setup(x => x.Deserialize<SyncItem>(Moq.It.IsAny<string>())).Throws<NullReferenceException>();

            fileSystemAccessorMock=new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.IsFileExists(Moq.It.IsAny<string>())).Returns(true);
            fileSystemAccessorMock.Setup(x => x.GetFilesInDirectory(Moq.It.IsAny<string>()))
                .Returns(new[] {interviewId.FormatGuid()});
            fileSystemAccessorMock.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Returns<string, string>(Path.Combine);

            unhandledPackageStorageMock = new Mock<IUnhandledPackageStorage>();
            incomingSyncPackagesQueue = CreateIncomingPackagesQueue(fileSystemAccessor: fileSystemAccessorMock.Object,
                jsonUtils: jsonUtilsMock.Object, unhandledPackageStorage: unhandledPackageStorageMock.Object);
        };

        Because of = () =>
            result = incomingSyncPackagesQueue.DeQueue();

        It should_delete_package = () =>
           fileSystemAccessorMock.Verify(x => x.DeleteFile(interviewId.FormatGuid()), Times.Once);

        It should_copy_package_to_error_folder = () =>
           unhandledPackageStorageMock.Verify(x => x.StoreUnhandledPackage(interviewId.FormatGuid(), null, Moq.It.IsAny<NullReferenceException>()), Times.Once);

        It should_result_be_null = () =>
           result.ShouldBeNull();

        private static IncomingSyncPackagesQueue incomingSyncPackagesQueue;
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock;
        private static Mock<IUnhandledPackageStorage> unhandledPackageStorageMock;
        private static Guid interviewId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static IncomingSyncPackages result;
    }
}
