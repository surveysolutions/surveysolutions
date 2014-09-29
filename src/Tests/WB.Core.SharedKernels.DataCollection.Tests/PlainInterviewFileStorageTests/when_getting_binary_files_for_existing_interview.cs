using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.PlainInterviewFileStorageTests
{
    internal class when_getting_binary_files_for_existing_interview : PlainInterviewFileStorageTestContext
    {
        Establish context = () =>
        {
            FileSystemAccessorMock.Setup(x => x.IsDirectoryExists(Moq.It.IsAny<string>())).Returns(true);
            FileSystemAccessorMock.Setup(x => x.GetFilesInDirectory(Moq.It.IsAny<string>())).Returns(new[] { file1 });
            FileSystemAccessorMock.Setup(x => x.GetFileName(Moq.It.IsAny<string>())).Returns<string>(name => name);
            FileSystemAccessorMock.Setup(x => x.ReadAllBytes(file1)).Returns(data1);

            plainFileRepository = CreatePlainFileRepository(fileSystemAccessor: FileSystemAccessorMock.Object);
        };

        Because of = () => result = plainFileRepository.GetBinaryFilesForInterview(interviewId);

        It should_1_file_be_returned = () =>
            result.Count.ShouldEqual(1);

        It should_name_of_returned_file_be_equal_to_file1 = () =>
           result[0].FileName.ShouldEqual(file1);

        It should_data_of_returned_file_be_equal_to_data1 = () =>
            result[0].GetData().ShouldEqual(data1);

        private static PlainInterviewFileStorage plainFileRepository;

        private static readonly Mock<IFileSystemAccessor> FileSystemAccessorMock = CreateIFileSystemAccessorMock();

        private static Guid interviewId = Guid.NewGuid();

        private static IList<InterviewBinaryDataDescriptor> result;
        private static string file1 = "file1";
        private static byte[] data1 = new byte[] { 1 };
    }
}
