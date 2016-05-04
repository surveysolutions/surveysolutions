using System;
using Machine.Specifications;
using Ncqrs.Eventing;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.EventBus.Lite.Implementation;

namespace WB.Tests.Unit.Infrastructure.LiteEventBusTests
{
    internal class when_publishing_event_and_bus_does_not_have_any_subscriptions : LiteEventBusTestsContext
    {
        Establish context = () =>
        {
            eventsToPublish = BuildReadyToBePublishedStream(Guid.NewGuid(), new DummyEvent());

            eventBus = Create.LiteEventBus();
        };

        Because of = () =>
            exception = Catch.Exception(() => eventBus.PublishCommittedEvents(eventsToPublish));

        It should_nothing_happen_including_exceptions = () =>
            exception.ShouldBeNull();


        private static LiteEventBus eventBus;
        private static CommittedEventStream eventsToPublish;
        private static Exception exception;
    }
}