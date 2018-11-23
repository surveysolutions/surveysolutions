using System;
using System.Collections.Generic;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Infrastructure.NcqrCompatibleEventDispatcherTests
{
    [Ignore("no need")]
    internal class when_commiting_ignored_aggregate_events : NcqrCompatibleEventDispatcherTestContext
    {
        [Test]
        public void should_commit_events_to_in_memory_storage()
        {
            var eventSourceToIgnore = Guid.NewGuid();
            var eventStreamReturnedByInMemoryStorage = new CommittedEventStream(eventSourceToIgnore);

            var aggregate = Mock.Of<IEventSourcedAggregateRoot>(x => x.EventSourceId == eventSourceToIgnore &&
                                                                     x.GetUnCommittedChanges() == new List<UncommittedEvent>());
            var inMemoryEventStore =
                Mock.Of<IInMemoryEventStore>(x => x.Store(It.IsAny<UncommittedEventStream>()) == eventStreamReturnedByInMemoryStorage);

            var eventStore = new Mock<IEventStore>();
            var bus =
                Create.Service.NcqrCompatibleEventDispatcher(eventBusSettings: new EventBusSettings
                    {
                        IgnoredAggregateRoots = new List<string>(new[] {eventSourceToIgnore.FormatGuid()})
                    }
                    //,inMemoryEventStore: inMemoryEventStore
                    );

            // Act
            CommittedEventStream result = null;//bus.CommitUncommittedEvents(aggregate, null);

            // Assert
            Assert.That(result, Is.SameAs(eventStreamReturnedByInMemoryStorage));
            eventStore.Verify(x => x.Store(It.IsAny<UncommittedEventStream>()), Times.Never);
        }
    }
}
