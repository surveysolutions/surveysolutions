using System;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Tests.Unit.SharedKernels.SurveyManagement.SyncPackagesProcessorTests;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.IncomingPackagesQueueTests
{
    internal class when_PushSyncItem_called_and_item_content_is_not_empty : IncomingPackagesQueueTestContext
    {
        Establish context = () =>
        {
            fileSystemAccessorMock = CreateDefaultFileSystemAccessorMock();

            incomingSyncPackagesQueue = CreateIncomingPackagesQueue(fileSystemAccessor: fileSystemAccessorMock.Object);
        };

        Because of = () =>
            incomingSyncPackagesQueue.Enqueue(Guid.Empty, contentOfSyncItem);

        It should_write_text_file_to_error_folder = () =>
          fileSystemAccessorMock.Verify(x => x.WriteAllText(Moq.It.IsAny<string>(), contentOfSyncItem), Times.Once);

        private static IncomingSyncPackagesQueue incomingSyncPackagesQueue;
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock;
        private static string contentOfSyncItem = "content of sync item";
    }
}
