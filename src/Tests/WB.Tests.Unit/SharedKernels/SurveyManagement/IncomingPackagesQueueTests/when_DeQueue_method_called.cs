using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization.IncomePackagesRepository;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.IncomingPackagesQueueTests
{
    internal class when_DeQueue_method_called : IncomingPackagesQueueTestContext
    {
        Establish context = () =>
        {
            fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.GetFilesInDirectory(Moq.It.IsAny<string>()))
                .Returns(filesInFolder);

            incomingPackagesQueue = CreateIncomingPackagesQueue(fileSystemAccessor: fileSystemAccessorMock.Object);
        };

        Because of = () =>
            result = incomingPackagesQueue.DeQueue();

        It should_result_be_equal_to_first_file_in_folder = () =>
           result.ShouldEqual(filesInFolder.First());

        private static IncomingPackagesQueue incomingPackagesQueue;
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock;

        private static string result;
        private static string[] filesInFolder = new[] { "f1", "f2" };
    }
}
