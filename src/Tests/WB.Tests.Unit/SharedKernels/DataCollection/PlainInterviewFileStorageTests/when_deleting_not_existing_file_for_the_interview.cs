using System;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Repositories;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;


namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainInterviewFileStorageTests
{
    internal class when_deleting_not_existing_file_for_the_interview : ImageQuestionFileStorageTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            imageFileRepository = CreatePlainFileRepository(fileSystemAccessor: FileSystemAccessorMock.Object);
            BecauseOf();
        }

        public void BecauseOf() => imageFileRepository.RemoveInterviewBinaryData(interviewId, fileName1);

        [NUnit.Framework.Test] public void should_file_be_never_deleted_from_file_system () =>
                FileSystemAccessorMock.Verify(x => x.DeleteFile(Moq.It.Is<string>(name => name.Contains(fileName1) && name.Contains(interviewId.FormatGuid()))), Times.Never);

        private static ImageFileStorage imageFileRepository;
        private static readonly Mock<IFileSystemAccessor> FileSystemAccessorMock = CreateIFileSystemAccessorMock();
        private static Guid interviewId = Guid.NewGuid();
        private static string fileName1 = "file1";
    }
}
