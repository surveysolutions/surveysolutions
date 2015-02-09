using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SyncPackagesProcessorTests
{
    internal class when_DeQueue_called_and_folder_with_packages_is_empty : SyncPackagesProcessorTestContext
    {
        Establish context = () =>
        {
            fileSystemMock = new Mock<IFileSystemAccessor>();

            syncPackagesProcessor = CreateSyncPackagesProcessor(fileSystemAccessor: fileSystemMock.Object);
        };

        Because of = () => syncPackagesProcessor.ProcessNextSyncPackage();

        It should_not_delete_any_files = () =>
          fileSystemMock.Verify(x=>x.DeleteFile(Moq.It.IsAny<string>()), Times.Never);

        private static SyncPackagesProcessor syncPackagesProcessor;

        private static Mock<IFileSystemAccessor> fileSystemMock;
    }
}
