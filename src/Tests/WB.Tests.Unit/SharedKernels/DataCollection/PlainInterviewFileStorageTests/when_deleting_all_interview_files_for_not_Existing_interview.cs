using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;


namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainInterviewFileStorageTests
{
    internal class when_deleting_all_interview_files_for_not_existing_interview : ImageQuestionFileStorageTestContext
    {
        public async Task should_interview_folder_be_never_deleted_from_file_system()
        {
            Guid interviewId = Guid.NewGuid();
            Mock<IFileSystemAccessor> fileSystemAccessorMock = CreateIFileSystemAccessorMock();
            var imageFileRepository = CreatePlainFileRepository(fileSystemAccessor: fileSystemAccessorMock.Object);
            
            await imageFileRepository.RemoveAllBinaryDataForInterviewsAsync(new List<Guid>(){ interviewId });
            
            fileSystemAccessorMock.Verify(x => x.DeleteDirectory(Moq.It.Is<string>(name => name.Contains(interviewId.FormatGuid()))), Times.Never);
        }
    }
}
