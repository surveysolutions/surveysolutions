using System;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainInterviewFileStorageTests
{
    internal class when_getting_data_for_existing_file : PlainInterviewFileStorageTestContext
    {
        Establish context = () =>
        {
            FileSystemAccessorMock.Setup(x => x.IsFileExists(Moq.It.IsAny<string>())).Returns(true);
            FileSystemAccessorMock.Setup(x => x.ReadAllBytes(Moq.It.IsAny<string>())).Returns(data1);
            fileSystemFileSystemFileRepository = CreatePlainFileRepository(fileSystemAccessor: FileSystemAccessorMock.Object);
        };

        Because of = () => result =fileSystemFileSystemFileRepository.GetInterviewBinaryData(interviewId, fileName1);

        It should_result_Be_equal_to_data1 = () =>
            result.ShouldEqual(data1);

        private static FileSystemFileSystemInterviewFileStorage fileSystemFileSystemFileRepository;

        private static readonly Mock<IFileSystemAccessor> FileSystemAccessorMock = CreateIFileSystemAccessorMock();

        private static Guid interviewId = Guid.NewGuid();
        private static string fileName1 = "file1";
        private static byte[] data1 = new byte[] { 1 };

        private static byte[] result;
    }
}
