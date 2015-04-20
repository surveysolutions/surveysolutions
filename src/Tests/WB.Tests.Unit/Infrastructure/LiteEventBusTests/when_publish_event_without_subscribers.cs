using System;
using Machine.Specifications;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.Tests.Unit.Infrastructure.LiteEventBusTests
{
    internal class when_publish_event_without_subscribers : LiteEventBusTestsContext
    {
        Establish context = () =>
        {
            aggregateRoot = CreateDummyAggregateRoot();

            eventBus = Create.LiteEventBus();
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