using System;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainInterviewFileStorageTests
{
    internal class when_deleting_one_file_stored_for_the_interview : ImageQuestionFileStorageTestContext
    {
        Establish context = () =>
        {
            FileSystemAccessorMock.Setup(x => x.IsFileExists(Moq.It.IsAny<string>())).Returns(true);

            imageFileRepository = CreatePlainFileRepository(fileSystemAccessor: FileSystemAccessorMock.Object);

            imageFileRepository.StoreInterviewBinaryData(interviewId, fileName1, data1);
        };

        Because of = () => imageFileRepository.RemoveInterviewBinaryData(interviewId, fileName1);

        It should_file_be_deleted_from_file_system = () =>
            FileSystemAccessorMock.Verify(x=>x.DeleteFile(Moq.It.Is<string>(name=>name.Contains(fileName1) && name.Contains(interviewId.FormatGuid()))));

        private static ImageFileStorage imageFileRepository;

        private static readonly Mock<IFileSystemAccessor> FileSystemAccessorMock = CreateIFileSystemAccessorMock();

        private static Guid interviewId = Guid.NewGuid();
        private static string fileName1 = "file1";

        private static byte[] data1 = new byte[] { 1 };
    }
}
