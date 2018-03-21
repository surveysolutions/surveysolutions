using System;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Repositories;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.FilebasedPreloadedDataRepositoryTests
{
    internal class when_DeletePreloadedData_is_called : FilebasedPreloadedDataRepositoryTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            fileSystemAccessor = CreateIFileSystemAccessorMock();
            fileSystemAccessor.Setup(x => x.IsDirectoryExists(Moq.It.IsAny<string>())).Returns(true);
            fileSystemAccessor.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>())).Returns(path);

            filebasedPreloadedDataRepository = CreateFilebasedPreloadedDataRepository(fileSystemAccessor.Object);
            BecauseOf();
        }

        public void BecauseOf() => filebasedPreloadedDataRepository.DeletePreloadedData(csvFileId);

        [NUnit.Framework.Test] public void should_call_delete_directory () =>
            fileSystemAccessor.Verify(x => x.DeleteDirectory(path), Times.Once);
            
        private static Mock<IFileSystemAccessor> fileSystemAccessor;
        private static FilebasedPreloadedDataRepository filebasedPreloadedDataRepository;
        private static string csvFileId = Guid.Parse("11111111111111111111111111111111").FormatGuid();
        private static string path = "test";
    }
}
