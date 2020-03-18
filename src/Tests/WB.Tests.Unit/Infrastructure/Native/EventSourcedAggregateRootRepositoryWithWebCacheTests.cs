﻿using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus;
using WB.Infrastructure.Native.Storage;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Infrastructure.Native
{
    public class EventSourcedAggregateRootRepositoryWithWebCacheTests
    {
        [Test]
        public void should_evict_from_cache_if_aggregate_root_is_dirty()
        {
            var eventStore = new Mock<IEventStore>();
            var domainRepo = new Mock<IDomainRepository>();

            const int versionForNewObjects = 100;
            const int changedVersion = 200;

            domainRepo
                .Setup(dr =>
                    dr.Load(It.IsAny<Type>(), It.IsAny<Guid>(), It.IsAny<IEnumerable<CommittedEvent>>()))
                .Returns<Type, Guid, IEnumerable<CommittedEvent>>((type, id, ce)
                    => Mock.Of<IEventSourcedAggregateRoot>(ar => ar.EventSourceId == id && ar.Version == versionForNewObjects));

            var repo = new EventSourcedAggregateRootRepositoryWithWebCache(eventStore.Object, Mock.Of<IInMemoryEventStore>(), new EventBusSettings(),  domainRepo.Object,
                ServiceLocator.Current,
                new Stub.StubAggregateLock());

            var entity = repo.GetLatest(typeof(IEventSourcedAggregateRoot), Id.g1);

            Assert.IsNotNull(entity);
            var entityMock = Mock.Get(entity);

            // Apply version change for existing Id.g1 entity and mark it as DIRTY
            entityMock.Setup(e => e.HasUncommittedChanges()).Returns(true);
            entityMock.Setup(e => e.Version).Returns(changedVersion);

            Assert.That(entity.Version, Is.EqualTo(changedVersion), "Just to make sure that entityMock work as expected");

            // act
            entity = repo.GetLatest(typeof(IEventSourcedAggregateRoot), Id.g1);

            // assert
            Assert.That(entity.Version, Is.EqualTo(versionForNewObjects), "Dirty entity should be evicted and read from domainRepo");
        }

        [Test]
        public void should_read_ignored_aggregate_from_in_memory_event_store()
        {
            var aggregateRootId = Id.gA;
            var aggregateFromInMemoryEvents = Mock.Of<IEventSourcedAggregateRoot>();
            var committedEvents = new List<CommittedEvent>();

            var inMemoryEventStoreMock = new Mock<IInMemoryEventStore>();
            inMemoryEventStoreMock.Setup(x => x.Read(aggregateRootId, 0))
                .Returns(committedEvents);

            var domainRepositoryMock = new Mock<IDomainRepository>();
            domainRepositoryMock.Setup(x => x.Load(typeof(IEventSourcedAggregateRoot), aggregateRootId, committedEvents))
                .Returns(aggregateFromInMemoryEvents);

            var eventBusSettings = new EventBusSettings();
            eventBusSettings.IgnoredAggregateRoots.Add(aggregateRootId.FormatGuid());

            var repository = GetRepository(inMemoryEventStore: inMemoryEventStoreMock.Object,
                domainRepository: domainRepositoryMock.Object,
                eventBusSettings: eventBusSettings);
        
            // Act
            var aggregate = repository.GetLatest(typeof(IEventSourcedAggregateRoot), aggregateRootId);

            // Assert
            Assert.That(aggregate, Is.SameAs(aggregateFromInMemoryEvents));
        }

        private EventSourcedAggregateRootRepositoryWithWebCache GetRepository(EventBusSettings eventBusSettings = null,
            IDomainRepository domainRepository = null,
            IEventStore eventStore = null,
            IInMemoryEventStore inMemoryEventStore = null)
        {
            return new EventSourcedAggregateRootRepositoryWithWebCache(
                eventStore ?? Mock.Of<IEventStore>(), 
                inMemoryEventStore ?? Mock.Of<IInMemoryEventStore>(), 
                eventBusSettings ?? new EventBusSettings(), 
                domainRepository ?? Mock.Of<IDomainRepository>(),
                ServiceLocator.Current,
                new Stub.StubAggregateLock()
                );
        }
    }
}
