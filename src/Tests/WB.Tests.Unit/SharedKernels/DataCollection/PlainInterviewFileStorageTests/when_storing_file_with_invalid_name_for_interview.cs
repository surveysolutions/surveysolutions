using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainInterviewFileStorageTests
{
    internal class when_storing_file_with_invalid_name_for_interview : ImageQuestionFileStorageTestContext
    {
        [Test]
        public void should_throw_argument_exception()
        {
            FileSystemAccessorMock
                .Setup(x => x.IsInvalidFileName(fileName))
                .Returns(true);

            var imageFileRepository = CreatePlainFileRepository(fileSystemAccessor: FileSystemAccessorMock.Object);

            Action act = () => imageFileRepository.StoreInterviewBinaryData(interviewId, fileName, data, null);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*Invalid file name*");

            FileSystemAccessorMock.Verify(x => x.WriteAllBytes(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Never);
        }

        private static readonly Mock<IFileSystemAccessor> FileSystemAccessorMock = CreateIFileSystemAccessorMock();
        private static readonly Guid interviewId = Guid.NewGuid();
        private const string fileName = "../file.png";
        private static readonly byte[] data = new byte[] { 1 };
    }
}
