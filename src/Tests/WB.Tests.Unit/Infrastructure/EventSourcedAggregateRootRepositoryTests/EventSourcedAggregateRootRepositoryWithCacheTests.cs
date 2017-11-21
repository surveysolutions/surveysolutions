using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using NUnit.Framework;
using WB.Core.Infrastructure.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Infrastructure.EventSourcedAggregateRootRepositoryTests
{
    [TestFixture]
    internal class EventSourcedAggregateRootRepositoryWithCacheTests
    {
        [Test]
        public void When_cached_aggregate_has_uncommitted_changes_Then_returns_newly_loaded_aggregate()
        {
            // arrange
            var dirtyAggregate = Mock.Of<IEventSourcedAggregateRoot>(_
                => _.HasUncommittedChanges() == true);

            var domainRepository = Mock.Of<IDomainRepository>();
            Mock.Get(domainRepository).SetReturnsDefault(dirtyAggregate);

            var eventSourcedRepository = Create.Service.EventSourcedAggregateRootRepository(repository: domainRepository);

            // act - get twice
            eventSourcedRepository.GetLatest(dirtyAggregate.GetType(), Guid.Empty);
            eventSourcedRepository.GetLatest(dirtyAggregate.GetType(), Guid.Empty);

            // assert - expect load twice
            Mock.Get(domainRepository).Verify(
                repository => repository.Load(It.IsAny<Type>(), It.IsAny<Guid>(), It.IsAny<Snapshot>(), It.IsAny<IEnumerable<CommittedEvent>>()),
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

            var eventSourcedRepository = Create.Service.EventSourcedAggregateRootRepository(repository: domainRepository);

            // act - get twice
            eventSourcedRepository.GetLatest(cleanAggregate.GetType(), Guid.Empty);
            eventSourcedRepository.GetLatest(cleanAggregate.GetType(), Guid.Empty);

            // assert - expect load once
            Mock.Get(domainRepository).Verify(
                repository => repository.Load(It.IsAny<Type>(), It.IsAny<Guid>(), It.IsAny<Snapshot>(), It.IsAny<IEnumerable<CommittedEvent>>()),
                Times.Exactly(1));
        }
    }
}
