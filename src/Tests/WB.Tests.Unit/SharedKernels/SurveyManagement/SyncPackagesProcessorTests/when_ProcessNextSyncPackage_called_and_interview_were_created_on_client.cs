using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Events;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.Synchronization;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SyncPackagesProcessorTests
{
    internal class when_ProcessNextSyncPackage_called_and_interview_were_created_on_client : SyncPackagesProcessorTestContext
    {
        Establish context = () =>
        {
            unhandledPackageStorage = new Mock<IBrokenSyncPackagesStorage>();
            incomingSyncPackagesQueueMock = new Mock<IIncomingSyncPackagesQueue>();
            incomingSyncPackagesQueueMock.Setup(x => x.DeQueue())
                .Returns(new IncomingSyncPackage(interviewId, Guid.NewGuid(), Guid.NewGuid(), 1,
                    InterviewStatus.Completed, new object[0], true, "", "path"));
            commandServiceMock=new Mock<ICommandService>();
            syncPackagesProcessor = CreateSyncPackagesProcessor(commandService: commandServiceMock.Object, incomingSyncPackagesQueue: incomingSyncPackagesQueueMock.Object, brokenSyncPackagesStorage: unhandledPackageStorage.Object);
        };

        Because of = () =>
            syncPackagesProcessor.ProcessNextSyncPackage();

        It should_call_SynchronizeInterviewEvents = () =>
            commandServiceMock.Verify(x => x.Execute(Moq.It.Is<SynchronizeInterviewEventsCommand>(_ => _.CreatedOnClient && _.InterviewId == interviewId), "", false), Times.Once);

        It should_call_DeleteSyncItem = () =>
        incomingSyncPackagesQueueMock.Verify(x => x.DeleteSyncItem("path"), Times.Once);

        It should_never_call_StoreUnhandledPackage = () =>
            unhandledPackageStorage.Verify(x => x.StoreUnhandledPackage("path", interviewId, Moq.It.IsAny<Exception>()), Times.Never);

        private static readonly DateTime initialTimestamp = new DateTime(2012, 04, 22);
        private static SyncPackagesProcessor syncPackagesProcessor;
        private static Guid interviewId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Mock<ICommandService> commandServiceMock;
        private static Mock<IIncomingSyncPackagesQueue> incomingSyncPackagesQueueMock;
        private static Mock<IBrokenSyncPackagesStorage> unhandledPackageStorage;
    }
}
