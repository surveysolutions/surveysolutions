using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.Services;
using WB.Infrastructure.Native;
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

            var repo = new EventSourcedAggregateRootRepositoryWithWebCache(eventStore.Object,
                Create.Storage.InMemoryEventStore(),
                Create.Service.MockOfAggregatePrototypeService(),  
                domainRepo.Object,
                ServiceLocator.Current,
                new Stub.StubAggregateLock(), 
                Create.Storage.NewAggregateRootCache(),
                Options.Create(new SchedulerConfig()));

            var entity = repo.GetLatest(typeof(IEventSourcedAggregateRoot), Id.g1);

            ClassicAssert.IsNotNull(entity);
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
        public void should_read_prototype_aggregate_from_in_memory_event_store()
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

            var prototypeService = Mock.Of<IAggregateRootPrototypeService>(s => s.GetPrototypeType(aggregateRootId) == PrototypeType.Permanent);

            var repository = GetRepository(inMemoryEventStore: inMemoryEventStoreMock.Object,
                domainRepository: domainRepositoryMock.Object,
                prototypeService: prototypeService);
        
            // Act
            var aggregate = repository.GetLatest(typeof(IEventSourcedAggregateRoot), aggregateRootId);

            // Assert
            Assert.That(aggregate, Is.SameAs(aggregateFromInMemoryEvents));
        }

        private EventSourcedAggregateRootRepositoryWithWebCache GetRepository(
            EventBusSettings eventBusSettings = null,
            IDomainRepository domainRepository = null,
            IEventStore eventStore = null,
            IInMemoryEventStore inMemoryEventStore = null,
            IAggregateRootPrototypeService prototypeService = null)
        {
            return new EventSourcedAggregateRootRepositoryWithWebCache(
                eventStore: eventStore ?? Mock.Of<IEventStore>(),
                inMemoryEventStore: inMemoryEventStore ?? Mock.Of<IInMemoryEventStore>(),
                repository: domainRepository ?? Mock.Of<IDomainRepository>(),
                serviceLocator: ServiceLocator.Current,
                aggregateLock: new Stub.StubAggregateLock(),
                memoryCache: Create.Storage.NewAggregateRootCache(),
                prototypeService: prototypeService ?? Create.Service.MockOfAggregatePrototypeService(),
                schedulerOptions: Options.Create(new SchedulerConfig()));
        }
    }
}
