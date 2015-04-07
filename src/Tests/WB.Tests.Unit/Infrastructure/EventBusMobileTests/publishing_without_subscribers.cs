using System;
using Machine.Specifications;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.Tests.Unit.Infrastructure.EventBusMobileTests
{
    internal class publishing_without_subscribers : EventBusTestsContext
    {
        Establish context = () =>
        {
            aggregateRoot = CreateDummyAggregateRoot();

            eventBus = CreateEventBus();
        };

        private Because of = () =>
            exception = Catch.Exception(() => eventBus.PublishUncommitedEventsFromAggregateRoot(aggregateRoot, null));

        It should_nothing_happen_including_exceptions = () =>
            exception.ShouldBeNull();


        private static ILiteEventBus eventBus;
        private static IAggregateRoot aggregateRoot;
        private static Exception exception;
    }
}