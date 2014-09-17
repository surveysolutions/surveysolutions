using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Tests.PlainFileRepositoryTests;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.FileSyncRepositoryTests
{
    internal class when_2_files_of_interview_are_moving_to_sync_storage : FileSyncRepositoryTestContext
    {
        Establish context = () =>
        {
            plainFileRepositoryMock.Setup(x => x.GetBinaryFilesForInterview(interviewId))
                .Returns(new List<InterviewBinaryData>
                {
                    new InterviewBinaryData(interviewId, "file1", () => data1),
                    new InterviewBinaryData(interviewId, "file2", () => data2)
                });
            fileSyncRepository = CreateFileSyncRepository(plainFileRepository: plainFileRepositoryMock.Object,fileSystemAccessor: fileSystemAccessorMock.Object);
        };

        Because of = () => fileSyncRepository.MoveInterviewsBinaryDataToSyncFolder(interviewId);

        It should_sycn_storage_store_2_files = () =>
            fileSystemAccessorMock.Verify(x => x.WriteAllBytes(Moq.It.IsAny<string>(), Moq.It.IsAny<byte[]>()), Times.Exactly(2));

        It should_sycn_storage_store_first_file_with_InterviewId_equal_to_interviewId = () =>
            fileSystemAccessorMock.Verify(x => x.WriteAllBytes(Moq.It.Is<string>(name=> name.Contains(interviewId.FormatGuid())), data1), Times.Once);

        It should_sycn_storage_store_second_file_with_InterviewId_equal_to_interviewId = () =>
           fileSystemAccessorMock.Verify(x => x.WriteAllBytes(Moq.It.Is<string>(name => name.Contains(interviewId.FormatGuid())), data2), Times.Once);

        private static FileSyncRepository fileSyncRepository;
        private static Mock<IPlainFileRepository> plainFileRepositoryMock = new Mock<IPlainFileRepository>();
          private static Mock<IFileSystemAccessor> fileSystemAccessorMock = CreateIFileSystemAccessorMock();
        private static Guid interviewId = Guid.NewGuid();
        private static byte[] data1 = new byte[] { 1 };
        private static byte[] data2 = new byte[] { 2 };
    }
}
