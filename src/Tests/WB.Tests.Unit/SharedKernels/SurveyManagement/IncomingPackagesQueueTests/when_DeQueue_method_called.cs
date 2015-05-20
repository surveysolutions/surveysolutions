using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Events;
using Moq;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.Synchronization;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.IncomingPackagesQueueTests
{
    internal class when_DeQueue_method_called : IncomingPackagesQueueTestContext
    {
        Establish context = () =>
        {
            var jsonUtilsMock = new Mock<IJsonUtils>();
            jsonUtilsMock.Setup(x => x.Deserialize<SyncItem>(Moq.It.IsAny<string>())).Returns(new SyncItem() { RootId = interviewId });
            jsonUtilsMock.Setup(x => x.Deserialize<InterviewMetaInfo>(Moq.It.IsAny<string>())).Returns(new InterviewMetaInfo());
            jsonUtilsMock.Setup(x => x.Deserialize<AggregateRootEvent[]>(Moq.It.IsAny<string>())).Returns(new []{ new AggregateRootEvent() });


            fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.GetFilesInDirectory(Moq.It.IsAny<string>()))
                .Returns(filesInFolder);
            fileSystemAccessorMock.Setup(x => x.IsFileExists(Moq.It.IsAny<string>()))
              .Returns(true);

            incomingSyncPackagesQueue = CreateIncomingPackagesQueue(fileSystemAccessor: fileSystemAccessorMock.Object, jsonUtils: jsonUtilsMock.Object);
        };

        Because of = () =>
            result = incomingSyncPackagesQueue.DeQueue();

        It should_result_be_equal_to_first_file_in_folder = () =>
            result.PathToPackage.ShouldEqual(filesInFolder.First());

        It should_not_delete_sync_package = () =>
            fileSystemAccessorMock.Verify(x => x.DeleteFile(Moq.It.IsAny<string>()), Times.Never);

        private static IncomingSyncPackagesQueue incomingSyncPackagesQueue;
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock;
        private static Guid interviewId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static IncomingSyncPackage result;
        private static string[] filesInFolder = new[] { "f1", "f2" };
    }
}
