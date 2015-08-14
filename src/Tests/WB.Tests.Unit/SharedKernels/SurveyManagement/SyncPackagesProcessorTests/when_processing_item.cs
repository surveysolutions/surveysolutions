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
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.Synchronization;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SyncPackagesProcessorTests
{
    internal class when_processing_item : SyncPackagesProcessorTestContext
    {
        Establish context = () =>
        {
            var incomingPackagesQueue = Mock.Of<IIncomingSyncPackagesQueue>(_ => _.DeQueue() == new IncomingSyncPackage(interviewId, Guid.NewGuid(), Guid.NewGuid(), 1, InterviewStatus.Completed, new object[0], false, "", "path"));

            commandServiceMock = new Mock<ICommandService>();

            syncPackagesProcessor = CreateSyncPackagesProcessor(commandService: commandServiceMock.Object,
                incomingSyncPackagesQueue: incomingPackagesQueue);
        };

        Because of = () =>
            syncPackagesProcessor.ProcessNextSyncPackage();

        It should_SynchronizeInterviewEvents_should_be_called = () =>
           commandServiceMock.Verify(x => x.Execute(Moq.It.Is<SynchronizeInterviewEventsCommand>(_ => !_.CreatedOnClient && _.InterviewId == interviewId), ""), Times.Once);

        private static readonly DateTime initialTimestamp = new DateTime(2012, 04, 22);
        private static SyncPackagesProcessor syncPackagesProcessor;
        private static Guid interviewId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Mock<ICommandService> commandServiceMock;
    }
} 