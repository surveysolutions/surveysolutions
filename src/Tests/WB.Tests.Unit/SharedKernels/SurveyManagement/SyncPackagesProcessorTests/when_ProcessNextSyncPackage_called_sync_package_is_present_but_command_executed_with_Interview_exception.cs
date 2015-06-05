using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Synchronization;
using WB.Core.Synchronization;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SyncPackagesProcessorTests
{
    internal class when_ProcessNextSyncPackage_called_sync_package_is_present_but_command_executed_with_Interview_exception : SyncPackagesProcessorTestContext
    {
        Establish context = () =>
        {
            unhandledPackageStorage = new Mock<IBrokenSyncPackagesStorage>();
            incomingSyncPackagesQueueMock = new Mock<IIncomingSyncPackagesQueue>();
            incomingSyncPackagesQueueMock.Setup(x => x.DeQueue())
                .Returns(new IncomingSyncPackage(interviewId, Guid.NewGuid(), Guid.NewGuid(), 1,
                    InterviewStatus.Completed, new object[0], true, "", "path"));

            commandServiceMock = new Mock<ICommandService>();
            commandServiceMock.Setup(x => x.Execute(Moq.It.IsAny<SynchronizeInterviewEventsCommand>(), Moq.It.IsAny<string>()))
                .Throws(new InterviewException("test", InterviewDomainExceptionType.OtherUserIsResponsible));

            syncPackagesProcessor = CreateSyncPackagesProcessor(commandService: commandServiceMock.Object, incomingSyncPackagesQueue: incomingSyncPackagesQueueMock.Object, brokenSyncPackagesStorage: unhandledPackageStorage.Object);
        };

        Because of = () =>
            syncPackagesProcessor.ProcessNextSyncPackage();

        It should_call_StoreUnhandledPackageForInterviewInTypedFolder = () =>
            unhandledPackageStorage.Verify(x => x.StoreUnhandledPackageForInterviewInTypedFolder("path", interviewId, Moq.It.IsAny<InterviewException>(), "OtherUserIsResponsible"), Times.Once);

        It should_call_DeleteSyncItem = () =>
          incomingSyncPackagesQueueMock.Verify(x => x.DeleteSyncItem("path"), Times.Once);

        private static SyncPackagesProcessor syncPackagesProcessor;
        private static Guid interviewId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Mock<ICommandService> commandServiceMock;
        private static Mock<IBrokenSyncPackagesStorage> unhandledPackageStorage;
        private static Mock<IIncomingSyncPackagesQueue> incomingSyncPackagesQueueMock;
    }
}
