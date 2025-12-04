using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing;
using NUnit.Framework;
using WB.Core.Infrastructure.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Infrastructure.EventSourcedAggregateRootRepositoryTests
{
    [TestFixture]
    internal class EventSourcedAggregateRootRepositoryWithCacheCacheTests
    {
        [Test]
        public void When_cached_aggregate_has_uncommitted_changes_Then_returns_newly_loaded_aggregate()
        {
            // arrange
            var dirtyAggregate = Mock.Of<IEventSourcedAggregateRoot>(_
                => _.HasUncommittedChanges() == true);

            var domainRepository = Mock.Of<IDomainRepository>();
            Mock.Get(domainRepository).SetReturnsDefault(dirtyAggregate);

            var eventSourcedRepository = Create.Service.EventSourcedAggregateRootRepositoryWithWebCache(repository: domainRepository);

            // act - get twice
            eventSourcedRepository.GetLatest(dirtyAggregate.GetType(), Guid.Empty);
            eventSourcedRepository.GetLatest(dirtyAggregate.GetType(), Guid.Empty);

            // assert - expect load twice
            Mock.Get(domainRepository).Verify(
                repository => repository.Load(It.IsAny<Type>(), It.IsAny<Guid>(), It.IsAny<IEnumerable<CommittedEvent>>()),
                Times.Exactly(2));
        }

        [Test]
        public void When_cached_aggregate_has_no_uncommitted_changes_Then_returns_cached_aggregate()
        {
            // arrange
            var cleanAggregate = Mock.Of<IEventSourcedAggregateRoot>(_
                => _.HasUncommittedChanges() == false);

            var domainRepository = Mock.Of<IDomainRepository>();
            Mock.Get(domainRepository).SetReturnsDefault(cleanAggregate);

            var eventSourcedRepository = Create.Service.EventSourcedAggregateRootRepositoryWithCache(repository: domainRepository);

            // act - get twice
            eventSourcedRepository.GetLatest(cleanAggregate.GetType(), Guid.Empty);
            eventSourcedRepository.GetLatest(cleanAggregate.GetType(), Guid.Empty);

            // assert - expect load once
            Mock.Get(domainRepository).Verify(
                repository => repository.Load(It.IsAny<Type>(), It.IsAny<Guid>(), It.IsAny<IEnumerable<CommittedEvent>>()),
                Times.Exactly(1));
        }

        [Test]
        public void When_getting_100_aggregates_and_getting_same_100_aggregates_in_reverse_order_Then_repository_load_should_be_called_100_times()
        {
            // arrange
            Guid[] aggregateIds = Enumerable.Range(1, 100).Select(_ => Guid.NewGuid()).ToArray();
            Dictionary<Guid, IEventSourcedAggregateRoot> storedAggregates = aggregateIds
                .Select(aggregateId => Mock.Of<IEventSourcedAggregateRoot>(_ => _.EventSourceId == aggregateId))
                .ToDictionary(aggregate => aggregate.EventSourceId);

            var domainRepository = Mock.Of<IDomainRepository>();
            Mock.Get(domainRepository)
                .Setup(repository => repository.Load(It.IsAny<Type>(), It.IsAny<Guid>(), It.IsAny<IEnumerable<CommittedEvent>>()))
                .Returns<Type, Guid, IEnumerable<CommittedEvent>>(
                    (_, aggregateId, ___) => storedAggregates[aggregateId]);

            var eventSourcedRepository = Create.Service.EventSourcedAggregateRootRepositoryWithWebCache(repository: domainRepository);

            // act
            foreach (var aggregateId in aggregateIds)
            {
                eventSourcedRepository.GetLatest(typeof(IEventSourcedAggregateRoot), aggregateId);
            }

            for (int i = aggregateIds.Length - 1; i >= 0; i--)
            {
                eventSourcedRepository.GetLatest(typeof(IEventSourcedAggregateRoot), aggregateIds[i]);
            }

            // assert
            Mock.Get(domainRepository).Verify(
                repository => repository.Load(It.IsAny<Type>(), It.IsAny<Guid>(), It.IsAny<IEnumerable<CommittedEvent>>()),
                Times.Exactly(100));
        }
    }
}
