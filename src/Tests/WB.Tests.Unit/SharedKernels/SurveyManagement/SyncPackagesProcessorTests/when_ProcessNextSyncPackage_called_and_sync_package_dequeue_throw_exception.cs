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
    internal class when_ProcessNextSyncPackage_called_and_sync_package_dequeue_throw_exception : SyncPackagesProcessorTestContext
    {
        Establish context = () =>
        {
            unhandledPackageStorage = new Mock<IBrokenSyncPackagesStorage>();
            incomingSyncPackagesQueueMock = new Mock<IIncomingSyncPackagesQueue>();
            incomingSyncPackagesQueueMock.Setup(x => x.DeQueue())
                .Throws(new IncomingSyncPackageException("some message", new NullReferenceException(), null, "path"));

            syncPackagesProcessor = CreateSyncPackagesProcessor(incomingSyncPackagesQueue: incomingSyncPackagesQueueMock.Object, brokenSyncPackagesStorage: unhandledPackageStorage.Object);
        };

        Because of = () =>
            syncPackagesProcessor.ProcessNextSyncPackage();

        It should_call_StoreUnknownUnhandledPackage = () =>
            unhandledPackageStorage.Verify(x => x.StoreUnknownUnhandledPackage("path", Moq.It.IsAny<NullReferenceException>()), Times.Once);

        private static SyncPackagesProcessor syncPackagesProcessor;
        private static Mock<IBrokenSyncPackagesStorage> unhandledPackageStorage;
        private static Mock<IIncomingSyncPackagesQueue> incomingSyncPackagesQueueMock;
    }
}
