using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Synchronization;
using WB.Core.Synchronization;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SyncPackagesProcessorTests
{
    internal class when_ProcessNextSyncPackage_called_and_sync_package_dequeue_throw_exception_with_interview_id : SyncPackagesProcessorTestContext
    {
        Establish context = () =>
        {
            unhandledPackageStorage = new Mock<IBrokenSyncPackagesStorage>();
            incomingSyncPackagesQueueMock = new Mock<IIncomingSyncPackagesQueue>();
            incomingSyncPackagesQueueMock.Setup(x => x.DeQueue())
                .Throws(new IncomingSyncPackageException("some message", new NullReferenceException(), interviewId, "path"));

            syncPackagesProcessor = CreateSyncPackagesProcessor(incomingSyncPackagesQueue: incomingSyncPackagesQueueMock.Object, brokenSyncPackagesStorage: unhandledPackageStorage.Object);
        };

        Because of = () =>
            syncPackagesProcessor.ProcessNextSyncPackage();

        It should_call_StoreUnhandledPackageForInterview = () =>
            unhandledPackageStorage.Verify(x => x.StoreUnhandledPackage("path", interviewId, Moq.It.IsAny<NullReferenceException>()), Times.Once);

        private static SyncPackagesProcessor syncPackagesProcessor;
        private static Guid interviewId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Mock<IBrokenSyncPackagesStorage> unhandledPackageStorage;
        private static Mock<IIncomingSyncPackagesQueue> incomingSyncPackagesQueueMock;
    }
}
