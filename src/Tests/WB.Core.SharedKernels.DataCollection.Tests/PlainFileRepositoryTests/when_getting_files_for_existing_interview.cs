using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.PlainFileRepositoryTests
{
    internal class when_getting_files_for_existing_interview : PlainFileRepositoryTestContext
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

        It should_result_count_Be_equal_to_1 = () =>
            result.Count.ShouldEqual(1);

        It should_result_first_item_file_name_be_equal_to_file1 = () =>
           result[0].FileName.ShouldEqual(file1);

        It should_result_first_item_data_be_equal_to_data1 = () =>
         result[0].Data.ShouldEqual(data1);

        private static PlainFileRepository plainFileRepository;

        private static readonly Mock<IFileSystemAccessor> FileSystemAccessorMock = CreateIFileSystemAccessorMock();

        private static Guid interviewId = Guid.NewGuid();

        private static IList<InterviewBinaryData> result;
        private static string file1 = "file1";
        private static byte[] data1 = new byte[] { 1 };
    }
}
