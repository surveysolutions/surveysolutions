using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.IncomingPackagesQueueTests
{
    internal class when_DeQueue_called_and_folder_with_packages_is_empty : IncomingPackagesQueueTestContext
    {
        Establish context = () =>
        {
            fileSystemMock = new Mock<IFileSystemAccessor>();

            incomingPackagesQueue = CreateIncomingPackagesQueue(fileSystemAccessor: fileSystemMock.Object);
        };

        Because of = () => incomingPackagesQueue.DeQueue();

        It should_not_delete_any_files = () =>
          fileSystemMock.Verify(x=>x.DeleteFile(Moq.It.IsAny<string>()), Times.Never);

        private static IncomingSyncPackagesQueue incomingPackagesQueue;

        private static Mock<IFileSystemAccessor> fileSystemMock;
    }
}
