using System;
using Machine.Specifications;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Tests.Unit.SharedKernels.SurveyManagement.SyncPackagesProcessorTests;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.IncomingPackagesQueueTests
{
    internal class when_PushSyncItem_called_and_item_has_empty_content : IncomingPackagesQueueTestContext
    {
        Establish context = () =>
        {
            incomingSyncPackagesQueue = CreateIncomingPackagesQueue();
        };

        Because of = () => exception = Catch.Exception(() =>
            incomingSyncPackagesQueue.Enqueue(Guid.Empty, ""));

        It should_throw_exception = () =>
          exception.ShouldNotBeNull();

        It should_throw_exception_of_type_ArgumentException = () =>
          exception.ShouldBeOfExactType<ArgumentException>();

        private static IncomingSyncPackagesQueue incomingSyncPackagesQueue;
        private static Exception exception;
    }
}
