using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainInterviewFileStorageTests
{
    internal class when_removing_file_with_invalid_name_for_interview : ImageQuestionFileStorageTestContext
    {
        [Test]
        public async System.Threading.Tasks.Task should_throw_argument_exception()
        {
            FileSystemAccessorMock
                .Setup(x => x.IsInvalidFileName(fileName))
                .Returns(true);

            var imageFileRepository = CreatePlainFileRepository(fileSystemAccessor: FileSystemAccessorMock.Object);

            Func<System.Threading.Tasks.Task> act = () => imageFileRepository.RemoveInterviewBinaryData(interviewId, fileName);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*Invalid file name*");

            FileSystemAccessorMock.Verify(x => x.DeleteFile(It.IsAny<string>()), Times.Never);
        }

        private static readonly Mock<IFileSystemAccessor> FileSystemAccessorMock = CreateIFileSystemAccessorMock();
        private static readonly Guid interviewId = Guid.NewGuid();
        private const string fileName = "../file.png";
    }
}
