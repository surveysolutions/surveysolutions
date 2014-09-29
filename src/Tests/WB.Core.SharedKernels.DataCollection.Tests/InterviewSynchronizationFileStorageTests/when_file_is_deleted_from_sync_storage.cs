using System;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewSynchronizationFileStorageTests
{
    internal class when_file_is_deleted_from_sync_storage : InterviewSynchronizationFileStorageTestContext
    {
        Establish context = () =>
        {
            fileSystemAccessorMock.Setup(x => x.IsFileExists(Moq.It.IsAny<string>())).Returns(true);
            interviewSynchronizationFileStorage = CreateFileSyncRepository(fileSystemAccessor:fileSystemAccessorMock.Object);
        };

        Because of = () => interviewSynchronizationFileStorage.RemoveBinaryDataFromSyncFolder(interviewId, fileName1);

        It should_one_file_be_deleted_from_file_system = () =>
            fileSystemAccessorMock.Verify(x=>x.DeleteFile(Moq.It.IsAny<string>()), Times.Once);

        private static InterviewSynchronizationFileStorage interviewSynchronizationFileStorage;
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock = CreateIFileSystemAccessorMock();
        private static Guid interviewId = Guid.NewGuid();
        private static string fileName1 = "file1";
    }
}
