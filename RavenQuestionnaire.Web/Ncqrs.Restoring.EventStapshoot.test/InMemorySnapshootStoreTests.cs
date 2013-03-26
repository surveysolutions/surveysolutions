using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using Ncqrs.Restoring.EventStapshoot.EventStores;

namespace Ncqrs.Restoring.EventStapshoot.test
{
    [TestFixture]
    public class InMemorySnapshootStoreTests
    {
        private Mock<ISnapshootEventStore> eventStore;
        private Mock<ISnapshotStore> snapshotStore;
        [SetUp]
        public void Prepare()
        {
            eventStore = new Mock<ISnapshootEventStore>();
            snapshotStore = new Mock<ISnapshotStore>();
        }

        [Test]
        public void SaveShapshot_SnapshotIsStoredToSubStore()
        {
            InMemorySnapshootStore target=new InMemorySnapshootStore(eventStore.Object, snapshotStore.Object);
            var snapshot = new Snapshot(Guid.NewGuid(), 1, new object());
            target.SaveShapshot(snapshot);
            snapshotStore.Verify(x => x.SaveShapshot(snapshot), Times.Once());

        }
        [Test]
        public void GetSnapshot_SnapshotIsPresentInSubStore_SnapshotIsStoredToSubStore()
        {
            InMemorySnapshootStore target = new InMemorySnapshootStore(eventStore.Object, snapshotStore.Object);
            var aggregateRootId = Guid.NewGuid();
            var snapshot = new Snapshot(aggregateRootId, 1, new object());
            snapshotStore.Setup(x => x.GetSnapshot(aggregateRootId, It.IsAny<long>())).Returns(snapshot);
            var result = target.GetSnapshot(aggregateRootId, 1);
            snapshotStore.Verify(x => x.GetSnapshot(aggregateRootId, 1), Times.Once());
            Assert.AreEqual(result, snapshot);

        }
        [Test]
        public void GetSnapshot_SnapshotIsAbsentInSubStore_SnapshotIsStoredToSubStore()
        {
            InMemorySnapshootStore target = new InMemorySnapshootStore(eventStore.Object, snapshotStore.Object);
            var aggregateRootId = Guid.NewGuid();
            var snapshot = new Snapshot(aggregateRootId, 1, new object());
            eventStore.Setup(x => x.GetLatestSnapshoot(aggregateRootId))
                      .Returns(new CommittedEvent(Guid.NewGuid(), Guid.NewGuid(), aggregateRootId, 1, DateTime.Now,
                                                  new SnapshootLoaded() {Template = snapshot}, new Version(1, 1)));
            var result = target.GetSnapshot(aggregateRootId, 1);
            eventStore.Verify(x => x.GetLatestSnapshoot(aggregateRootId), Times.Once());
            snapshotStore.Verify(x=>x.SaveShapshot(It.IsAny<Snapshot>()), Times.Once());
            Assert.AreEqual(result.Payload, snapshot.Payload);
            Assert.AreEqual(result.EventSourceId, snapshot.EventSourceId);
            Assert.AreEqual(result.Version, 1);

        }
        [Test]
        public void GetSnapshot_SnapshotIsAbsentInSubStoreAndEventStream_Null()
        {
            InMemorySnapshootStore target = new InMemorySnapshootStore(eventStore.Object, snapshotStore.Object);
            var aggregateRootId = Guid.NewGuid();
            var result = target.GetSnapshot(aggregateRootId, 1);
           
            Assert.AreEqual(result, null);

        }
    }

}
