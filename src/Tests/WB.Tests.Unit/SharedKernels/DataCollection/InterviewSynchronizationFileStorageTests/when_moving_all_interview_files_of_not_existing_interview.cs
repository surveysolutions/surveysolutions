using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewSynchronizationFileStorageTests
{
    internal class when_moving_all_interview_files_of_not_existing_interview : InterviewSynchronizationFileStorageTestContext
    {
        Establish context = () =>
        {
            plainFileRepositoryMock.Setup(x => x.GetBinaryFilesForInterview(interviewId))
             .Returns(new List<InterviewBinaryDataDescriptor>());
            interviewSynchronizationFileStorage = CreateFileSyncRepository(imageFileRepository: plainFileRepositoryMock.Object, fileSystemAccessor: fileSystemAccessorMock.Object);
        };

        Because of = () => interviewSynchronizationFileStorage.MoveInterviewImagesToSyncFolder(interviewId);

        It should_not_put_files_into_sync_storage = () =>
           fileSystemAccessorMock.Verify(x=>x.WriteAllBytes(Moq.It.IsAny<string>(), Moq.It.IsAny<byte[]>()), Times.Never);

        private static InterviewSynchronizationFileStorage interviewSynchronizationFileStorage;
        private static Mock<IImageFileStorage> plainFileRepositoryMock = new Mock<IImageFileStorage>();
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock = CreateIFileSystemAccessorMock();
        private static Guid interviewId = Guid.NewGuid();
    }
}
