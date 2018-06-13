using System;
using NUnit.Framework;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Infrastructure.LiteEventBusTests
{
    internal class when_publishing_event_and_bus_does_not_have_any_subscriptions : LiteEventBusTestsContext
    {
        [NUnit.Framework.Test] public void should_nothing_happen_including_exceptions () {
            var eventsToPublish = BuildReadyToBePublishedStream(Guid.NewGuid(), new DummyEvent());

            var eventBus = Create.Service.LiteEventBus();

            Assert.That(() => eventBus.PublishCommittedEvents(eventsToPublish), Throws.Nothing);
        }

    }
}
