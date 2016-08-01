using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.IncomingPackagesQueueTests
{
    internal class when_PushSyncItem_called_and_item_has_empty_content : IncomingPackagesQueueTestContext
    {
        Establish context = () =>
        {
            incomingSyncPackagesService = CreateIncomingPackagesQueue();
        };

        Because of = () => exception = Catch.Exception(() =>
            incomingSyncPackagesService.StoreOrProcessPackage(""));

        It should_throw_exception = () =>
          exception.ShouldNotBeNull();

        It should_throw_exception_of_type_ArgumentException = () =>
          exception.ShouldBeOfExactType<ArgumentException>();

        private static IncomingSyncPackagesService incomingSyncPackagesService;
        private static Exception exception;
    }
}
