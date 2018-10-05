using System;
using FluentAssertions;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;


namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainInterviewFileStorageTests
{
    internal class when_getting_data_for_existing_file : ImageQuestionFileStorageTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            FileSystemAccessorMock.Setup(x => x.IsFileExists(Moq.It.IsAny<string>())).Returns(true);
            FileSystemAccessorMock.Setup(x => x.ReadAllBytes(Moq.It.IsAny<string>(), null, null)).Returns(data1);
            imageFileRepository = CreatePlainFileRepository(fileSystemAccessor: FileSystemAccessorMock.Object);
            BecauseOf();
        }

        public void BecauseOf() => result =imageFileRepository.GetInterviewBinaryData(interviewId, fileName1);

        [NUnit.Framework.Test] public void should_result_Be_equal_to_data1 () =>
            result.Should().BeEquivalentTo(data1);

        private static ImageFileStorage imageFileRepository;

        private static readonly Mock<IFileSystemAccessor> FileSystemAccessorMock = CreateIFileSystemAccessorMock();

        private static Guid interviewId = Guid.NewGuid();
        private static string fileName1 = "file1";
        private static byte[] data1 = new byte[] { 1 };

        private static byte[] result;
    }
}
