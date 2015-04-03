using System;
using Machine.Specifications;
using WB.Core.Infrastructure.EventBus;

namespace WB.Tests.Unit.Infrastructure.EventBusMobileTests
{
    internal class publishing_without_subscribers : EventBusTestsContext
    {
        Establish context = () =>
        {
            value = 10;

            eventBus = CreateEventBus();
        };

        private Because of = () =>
            exception = Catch.Exception(() => eventBus.Publish(value));

        It should_nothing_happen_including_exceptions = () =>
            exception.ShouldBeNull();


        private static ILiteEventBus eventBus;
        private static int value;
        private static Exception exception;
    }
}