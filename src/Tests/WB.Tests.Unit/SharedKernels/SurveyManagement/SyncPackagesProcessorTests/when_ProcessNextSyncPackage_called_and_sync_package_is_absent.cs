using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SyncPackagesProcessorTests
{
    internal class when_ProcessNextSyncPackage_called_and_sync_package_is_absent : SyncPackagesProcessorTestContext
    {
        Establish context = () =>
        {
            commandServiceMock = new Mock<ICommandService>();
            syncPackagesProcessor = CreateSyncPackagesProcessor(commandService: commandServiceMock.Object);
        };

        Because of = () =>
            syncPackagesProcessor.ProcessNextSyncPackage();

        It should_never_call_SynchronizeInterviewEvents = () =>
            commandServiceMock.Verify(x => x.Execute(Moq.It.IsAny<SynchronizeInterviewEventsCommand>(), Moq.It.IsAny<string>()), Times.Never);

        private static readonly DateTime initialTimestamp = new DateTime(2012, 04, 22);
        private static SyncPackagesProcessor syncPackagesProcessor;
        private static Guid interviewId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Mock<ICommandService> commandServiceMock;
    }
}
