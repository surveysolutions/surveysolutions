using System;
using System.IO;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.Synchronization;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SyncPackagesProcessorTests
{
    internal class when_DeQueue_called_and_package_cant_be_opened : SyncPackagesProcessorTestContext
    {
        Establish context = () =>
        {
            var jsonUtilsMock = new Mock<IJsonUtils>();
            jsonUtilsMock.Setup(x => x.Deserialize<SyncItem>(Moq.It.IsAny<string>())).Throws<NullReferenceException>();

            fileSystemAccessorMock=new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.IsFileExists(Moq.It.IsAny<string>())).Returns(true);
            fileSystemAccessorMock.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                .Returns<string, string>(Path.Combine);

            var incomingPackagesQueue = Mock.Of<IIncomingSyncPackagesQueue>(_ => _.DeQueue() == interviewId.FormatGuid());

            unhandledPackageStorageMock = new Mock<IUnhandledPackageStorage>();
            syncPackagesProcessor = CreateSyncPackagesProcessor(fileSystemAccessor: fileSystemAccessorMock.Object, jsonUtils: jsonUtilsMock.Object,
                incomingSyncPackagesQueue: incomingPackagesQueue, unhandledPackageStorage: unhandledPackageStorageMock.Object);
        };

        Because of = () =>
            syncPackagesProcessor.ProcessNextSyncPackage();

        It should_delete_package = () =>
           fileSystemAccessorMock.Verify(x => x.DeleteFile(interviewId.FormatGuid()), Times.Once);

        It should_copy_package_to_error_folder = () =>
           unhandledPackageStorageMock.Verify(x => x.StoreUnhandledPackage(interviewId.FormatGuid(), null), Times.Once);

        private static SyncPackagesProcessor syncPackagesProcessor;
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock;
        private static Mock<IUnhandledPackageStorage> unhandledPackageStorageMock;
        private static Guid interviewId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
    }
}
