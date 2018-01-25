using System;
using System.Collections.Generic;
using Moq;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using NUnit.Framework;
using WB.Core.Infrastructure.Aggregates;
using WB.Infrastructure.Native.Monitoring;
using WB.Infrastructure.Native.Storage;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Infrastructure.Native
{
    public class EventSourcedAggregateRootRepositoryWithWebCacheTests
    {
        [Test]
        public void on_evict_should_not_call_cached_remove_method_twice()
        {
            var eventStore = new Mock<IEventStore>();
            var snapshotStore = new Mock<ISnapshotStore>();
            var domainRepo = new Mock<IDomainRepository>();

            domainRepo
                .Setup(dr => dr.Load(typeof(DumbEntity),It.IsAny<Guid>(), null, It.IsAny<IEnumerable<CommittedEvent>>()))
                .Returns<Type, Guid, Snapshot, IEnumerable<CommittedEvent>>((type, id, s, ce) => new DumbEntity(id));

            var repo = new EventSourcedAggregateRootRepositoryWithWebCache(eventStore.Object, snapshotStore.Object, domainRepo.Object,
                new Stub.StubAggregateLock());

            CommonMetrics.StateFullInterviewsCount.Set(0);

            repo.GetLatest(typeof(DumbEntity), Id.g1);
            Assert.That(CommonMetrics.StateFullInterviewsCount.Value, Is.EqualTo(1));

            var entity = repo.GetLatest(typeof(DumbEntity), Id.g2);
            Assert.That(CommonMetrics.StateFullInterviewsCount.Value, Is.EqualTo(2));
            Assert.NotNull(entity);

            repo.Evict(Id.g1);
            Assert.That(CommonMetrics.StateFullInterviewsCount.Value, Is.EqualTo(1));

            repo.Evict(Id.g1);
            Assert.That(CommonMetrics.StateFullInterviewsCount.Value, Is.EqualTo(1), "Should not decrease interviews counter");
        }

        private class DumbEntity : IEventSourcedAggregateRoot
        {
            public DumbEntity(Guid id)
            {
                EventSourceId = id;
            }

            public void SetId(Guid id)
            {
                throw new NotImplementedException();
            }

            public Guid EventSourceId { get; }
            public int Version { get; }
            public int InitialVersion { get; }
            public void InitializeFromHistory(Guid eventSourceId, IEnumerable<CommittedEvent> history)
            {
                throw new NotImplementedException();
            }

            public event EventHandler<EventAppliedEventArgs> EventApplied;
            public void AcceptChanges()
            {
                throw new NotImplementedException();
            }

            public bool HasUncommittedChanges()
            {
                throw new NotImplementedException();
            }

            public List<UncommittedEvent> GetUnCommittedChanges()
            {
                throw new NotImplementedException();
            }

            public void MarkChangesAsCommitted()
            {
                throw new NotImplementedException();
            }
        }

    }
}