using System;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainInterviewFileStorageTests
{
    internal class when_storing_file_for_interview : ImageQuestionFileStorageTestContext
    {
        Establish context = () =>
        {
            imageFileRepository = CreatePlainFileRepository(fileSystemAccessor: FileSystemAccessorMock.Object);
        };

        Because of = () => imageFileRepository.StoreInterviewBinaryData(interviewId, fileName1, data1);

        It should_file_be_stored_on_file_system_once = () =>
            FileSystemAccessorMock.Verify(x =>  x.WriteAllBytes(Moq.It.Is<string>(name => name.Contains(interviewId.FormatGuid())), data1), Times.Once);

        private static ImageFileStorage imageFileRepository;

        private static readonly Mock<IFileSystemAccessor> FileSystemAccessorMock = CreateIFileSystemAccessorMock();

        private static Guid interviewId = Guid.NewGuid();
        private static string fileName1 = "file1";
        private static byte[] data1 = new byte[] { 1 };
    }
}
