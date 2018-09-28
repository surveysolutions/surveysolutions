using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;


namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainInterviewFileStorageTests
{
    internal class when_getting_binary_files_for_existing_interview : ImageQuestionFileStorageTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            FileSystemAccessorMock.Setup(x => x.IsDirectoryExists(Moq.It.IsAny<string>())).Returns(true);
            FileSystemAccessorMock.Setup(x => x.GetFilesInDirectory(Moq.It.IsAny<string>(), Moq.It.IsAny<bool>())).Returns(new[] { file1 });
            FileSystemAccessorMock.Setup(x => x.GetFileName(Moq.It.IsAny<string>())).Returns<string>(name => name);
            FileSystemAccessorMock.Setup(x => x.ReadAllBytes(file1, null, null)).Returns(data1);

            imageFileRepository = CreatePlainFileRepository(fileSystemAccessor: FileSystemAccessorMock.Object);
            BecauseOf();
        }

        public void BecauseOf() => result = imageFileRepository.GetBinaryFilesForInterview(interviewId);

        [NUnit.Framework.Test] public void should_1_file_be_returned () =>
            result.Count.Should().Be(1);

        [NUnit.Framework.Test] public void should_name_of_returned_file_be_equal_to_file1 () =>
           result[0].FileName.Should().Be(file1);

        [NUnit.Framework.Test] public void should_data_of_returned_file_be_equal_to_data1 () =>
            result[0].GetData().Should().BeEquivalentTo(data1);

        private static ImageFileStorage imageFileRepository;

        private static readonly Mock<IFileSystemAccessor> FileSystemAccessorMock = CreateIFileSystemAccessorMock();

        private static Guid interviewId = Guid.NewGuid();

        private static IList<InterviewBinaryDataDescriptor> result;
        private static string file1 = "file1";
        private static byte[] data1 = new byte[] { 1 };
    }
}
