using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.PlainFileRepositoryTests
{
    internal class when_deleting_not_existing_file_for_the_interview : PlainFileRepositoryTestContext
    {
        Establish context = () =>
        {
            plainFileRepository = CreatePlainFileRepository(fileSystemAccessor: FileSystemAccessorMock.Object);
        };

        Because of = () => plainFileRepository.RemoveInterviewBinaryData(interviewId, fileName1);

        It should_file_be_never_deleted_from_file_system = () =>
                FileSystemAccessorMock.Verify(x => x.DeleteFile(Moq.It.Is<string>(name => name.Contains(fileName1) && name.Contains(interviewId.FormatGuid()))), Times.Never);

        private static PlainFileRepository plainFileRepository;
        private static readonly Mock<IFileSystemAccessor> FileSystemAccessorMock = CreateIFileSystemAccessorMock();
        private static Guid interviewId = Guid.NewGuid();
        private static string fileName1 = "file1";
    }
}
