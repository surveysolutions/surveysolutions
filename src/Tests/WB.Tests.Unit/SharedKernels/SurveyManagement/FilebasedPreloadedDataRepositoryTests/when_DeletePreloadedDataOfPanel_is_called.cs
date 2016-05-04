using System;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.FilebasedPreloadedDataRepositoryTests
{
    internal class when_DeletePreloadedDataOfPanel_is_called : FilebasedPreloadedDataRepositoryTestContext
    {
        private Establish context = () =>
        {
            fileSystemAccessor = CreateIFileSystemAccessorMock();
            fileSystemAccessor.Setup(x => x.IsDirectoryExists(Moq.It.IsAny<string>())).Returns(true);
            fileSystemAccessor.Setup(x => x.CombinePath(Moq.It.IsAny<string>(), Moq.It.IsAny<string>())).Returns(path);

            filebasedPreloadedDataRepository = CreateFilebasedPreloadedDataRepository(fileSystemAccessor.Object);
        };

        Because of = () => filebasedPreloadedDataRepository.DeletePreloadedDataOfPanel(csvFileId);
        
        private It should_call_delete_directory = () =>
            fileSystemAccessor.Verify(x => x.DeleteDirectory(path), Times.Once);
            
        private static Mock<IFileSystemAccessor> fileSystemAccessor;
        private static FilebasedPreloadedDataRepository filebasedPreloadedDataRepository;
        private static string csvFileId = Guid.Parse("11111111111111111111111111111111").FormatGuid();
        private static string path = "test";
    }
}
