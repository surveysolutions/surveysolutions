using System;
using Machine.Specifications;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.Tests.Unit.Infrastructure.LiteEventBusTests
{
    internal class when_publishing_event_and_bus_does_not_have_any_subscriptions : LiteEventBusTestsContext
    {
        Establish context = () =>
        {
            aggregateRoot = SetupAggregateRootWithOneEventReadyForPublishing<DummyEvent>();

            eventBus = Create.LiteEventBus();
        };

        Because of = () =>
            exception = Catch.Exception(() => eventBus.PublishUncommittedEvents(aggregateRoot));

        It should_nothing_happen_including_exceptions = () =>
            exception.ShouldBeNull();


        private static ILiteEventBus eventBus;
        private static IAggregateRoot aggregateRoot;
        private static Exception exception;
    }
}