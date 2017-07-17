using System;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainInterviewFileStorageTests
{
    internal class when_deleting_all_files_stored_for_the_interview : PlainInterviewFileStorageTestContext
    {
        Establish context = () =>
        {
            fileSystemFileSystemFileRepository = CreatePlainFileRepository(fileSystemAccessor: FileSystemAccessorMock.Object);

            FileSystemAccessorMock.Setup(x => x.IsDirectoryExists(Moq.It.IsAny<string>())).Returns(true);
        };

        Because of = () => fileSystemFileSystemFileRepository.RemoveAllBinaryDataForInterview(interviewId);


        It should_interview_folder_be_deleted_from_file_system = () =>
          FileSystemAccessorMock.Verify(x => x.DeleteDirectory(Moq.It.Is<string>(name => name.Contains(interviewId.FormatGuid()))), Times.Once);

        private static FileSystemFileSystemInterviewFileStorage fileSystemFileSystemFileRepository;

        private static readonly Mock<IFileSystemAccessor> FileSystemAccessorMock = CreateIFileSystemAccessorMock();

        private static Guid interviewId = Guid.NewGuid();
    }
}
