using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewSynchronizationFileStorageTests
{
    internal class when_2_interview_files_are_moved_to_sync_storage : InterviewSynchronizationFileStorageTestContext
    {
        Establish context = () =>
        {
            plainFileRepositoryMock.Setup(x => x.GetBinaryFilesForInterview(interviewId))
                .Returns(new List<InterviewBinaryDataDescriptor>
                {
                    new InterviewBinaryDataDescriptor(interviewId, "file1", () => data1),
                    new InterviewBinaryDataDescriptor(interviewId, "file2", () => data2)
                });
            interviewSynchronizationFileStorage = CreateFileSyncRepository(plainFileRepository: plainFileRepositoryMock.Object,fileSystemAccessor: fileSystemAccessorMock.Object);
        };

        Because of = () => interviewSynchronizationFileStorage.MoveInterviewsBinaryDataToSyncFolder(interviewId);

        It should_store_2_files_in_sync_storage = () =>
            fileSystemAccessorMock.Verify(x => x.WriteAllBytes(Moq.It.IsAny<string>(), Moq.It.IsAny<byte[]>()), Times.Exactly(2));

        It should_store_first_file_with_InterviewId_in_sync_storage = () =>
            fileSystemAccessorMock.Verify(x => x.WriteAllBytes(Moq.It.Is<string>(name=> name.Contains(interviewId.FormatGuid())), data1), Times.Once);

        It should_store_second_file_with_InterviewId_in_sync_storage = () =>
           fileSystemAccessorMock.Verify(x => x.WriteAllBytes(Moq.It.Is<string>(name => name.Contains(interviewId.FormatGuid())), data2), Times.Once);

        private static InterviewSynchronizationFileStorage interviewSynchronizationFileStorage;
        private static Mock<IPlainInterviewFileStorage> plainFileRepositoryMock = new Mock<IPlainInterviewFileStorage>();
         private static Mock<IFileSystemAccessor> fileSystemAccessorMock = CreateIFileSystemAccessorMock();
        private static Guid interviewId = Guid.NewGuid();
        private static byte[] data1 = new byte[] { 1 };
        private static byte[] data2 = new byte[] { 2 };
    }
}
