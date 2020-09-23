using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;


namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainInterviewFileStorageTests
{
    internal class when_deleting_all_files_stored_for_the_interview : ImageQuestionFileStorageTestContext
    {
        [Test]
        public async Task should_interview_folder_be_deleted_from_file_system()
        {
            Guid interviewId = Guid.NewGuid();
            Mock<IFileSystemAccessor> fileSystemAccessorMock = CreateIFileSystemAccessorMock();
            var imageFileRepository = CreatePlainFileRepository(fileSystemAccessor: fileSystemAccessorMock.Object);
            fileSystemAccessorMock.Setup(x => x.IsDirectoryExists(Moq.It.IsAny<string>())).Returns(true);

            await imageFileRepository.RemoveAllBinaryDataForInterviewsAsync(new List<Guid>(){ interviewId });
            
            fileSystemAccessorMock.Verify(x => x.DeleteDirectory(Moq.It.Is<string>(name => name.Contains(interviewId.FormatGuid()))), Times.Once);
        }
    }
}
