using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization.IncomePackagesRepository;
using WB.Tests.Unit.SharedKernels.SurveyManagement.SyncPackagesProcessorTests;
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

            incomingPackagesQueue = CreateIncomingPackagesQueue(fileSystemAccessor: fileSystemAccessorMock.Object);
        };

        Because of = () =>
            result = incomingPackagesQueue.QueueLength;

        It should_result_length_of_queue_be_equal_to_cound_of_files_in_folder = () =>
           result.ShouldEqual(2);

        private static IncomingPackagesQueue incomingPackagesQueue;
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock;
       
        private static int result;
        private static string[] filesInFolder = new[] { "f1", "f2" };
    }
}
