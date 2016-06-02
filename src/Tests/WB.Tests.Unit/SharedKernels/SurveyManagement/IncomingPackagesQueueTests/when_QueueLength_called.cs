using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.Infrastructure.FileSystem;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.IncomingPackagesQueueTests
{
    internal class when_QueueLength_called : IncomingPackagesQueueTestContext
    {
        Establish context = () =>
        {
            fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.GetFilesInDirectory(Moq.It.IsAny<string>()))
                .Returns(filesInFolder);

            incomingSyncPackagesService = CreateIncomingPackagesQueue(fileSystemAccessor: fileSystemAccessorMock.Object);
        };

        Because of = () =>
            result = incomingSyncPackagesService.QueueLength;

        It should_result_length_of_queue_be_equal_to_cound_of_files_in_folder = () =>
           result.ShouldEqual(2);

        private static IncomingSyncPackagesService incomingSyncPackagesService;
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock;
       
        private static int result;
        private static string[] filesInFolder = new[] { "f1.sync", "f2.sync" };
    }
}
