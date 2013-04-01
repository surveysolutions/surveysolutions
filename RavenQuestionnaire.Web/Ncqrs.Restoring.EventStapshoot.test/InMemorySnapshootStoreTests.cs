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
        private Mock<ISnapshotStore> uncommitedSnapshotStore;
     //   private Mock<ISnapshotStore> commitedSnapshotStore;
        [SetUp]
        public void Prepare()
        {
            eventStore = new Mock<ISnapshootEventStore>();
            uncommitedSnapshotStore = new Mock<ISnapshotStore>();
          //  commitedSnapshotStore = new Mock<ISnapshotStore>();
        }

        [Test]
        public void SaveShapshot_SnapshotIsStoredToSubStore()
        {
            InMemorySnapshootStore target = new InMemorySnapshootStore(eventStore.Object, uncommitedSnapshotStore.Object);
            var snapshot = new Snapshot(Guid.NewGuid(), 1, new object());
            target.SaveShapshot(snapshot);
            uncommitedSnapshotStore.Verify(x => x.SaveShapshot(snapshot), Times.Once());

        }
        [Test]
        public void GetSnapshot_SnapshotIsPresentInSubStore_SnapshotIsStoredToSubStore()
        {
            InMemorySnapshootStore target = new InMemorySnapshootStore(eventStore.Object, uncommitedSnapshotStore.Object);
            var aggregateRootId = Guid.NewGuid();
            var snapshot = new Snapshot(aggregateRootId, 1, new object());
            uncommitedSnapshotStore.Setup(x => x.GetSnapshot(aggregateRootId, It.IsAny<long>())).Returns(snapshot);
            var result = target.GetSnapshot(aggregateRootId, 1);
            uncommitedSnapshotStore.Verify(x => x.GetSnapshot(aggregateRootId, 1), Times.Once());
            Assert.AreEqual(result, snapshot);

        }
     /*   [Test]
        public void GetSnapshot_SnapshotIsAbsentInSubStore_SnapshotIsStoredToSubStoreForCommitedEvents()
        {
            InMemorySnapshootStore target = new InMemorySnapshootStore(eventStore.Object, uncommitedSnapshotStore.Object);
            var aggregateRootId = Guid.NewGuid();
            var snapshot = new Snapshot(aggregateRootId, 1, new object());
            eventStore.Setup(x => x.GetLatestSnapshoot(aggregateRootId))
                      .Returns(new CommittedEvent(Guid.NewGuid(), Guid.NewGuid(), aggregateRootId, 1, DateTime.Now,
                                                  new SnapshootLoaded() {Template = snapshot}, new Version(1, 1)));
            var result = target.GetSnapshot(aggregateRootId, 1);
            eventStore.Verify(x => x.GetLatestSnapshoot(aggregateRootId), Times.Once());
            uncommitedSnapshotStore.Verify(x=>x.SaveShapshot(It.IsAny<Snapshot>()), Times.Never());
            Assert.IsTrue(result is CommitedSnapshot);
            Assert.AreEqual(result.Payload, snapshot.Payload);
            Assert.AreEqual(result.EventSourceId, snapshot.EventSourceId);
            Assert.AreEqual(result.Version, 1);

            //chech that next request wouldn't call GetLatestSnapshoot one more time
            result = target.GetSnapshot(aggregateRootId, 1);
            eventStore.Verify(x => x.GetLatestSnapshoot(aggregateRootId), Times.Once());

        }*/
        [Test]
        public void GetSnapshot_SnapshotIsAbsentInSubStoreAndEventStream_Null()
        {
            InMemorySnapshootStore target = new InMemorySnapshootStore(eventStore.Object, uncommitedSnapshotStore.Object);
            var aggregateRootId = Guid.NewGuid();
            var result = target.GetSnapshot(aggregateRootId, 1);
           
            Assert.AreEqual(result, null);

        }
     /*   [Test]
        public void GetSnapshot_SnapshotSavedToMemoryAndEventStore_ReturnedSnapshotIsFromPersistanceStore()
        {
            InMemorySnapshootStore target = new InMemorySnapshootStore(eventStore.Object, uncommitedSnapshotStore.Object);
            var aggregateRootId = Guid.NewGuid();
            var targetSnpshot = new Snapshot(aggregateRootId, 1, new object());
            var snpshotEvent = new SnapshootLoaded() {Template = targetSnpshot};

            eventStore.Setup(x => x.GetLatestSnapshoot(aggregateRootId))
                      .Returns(new CommittedEvent(Guid.NewGuid(), Guid.NewGuid(), aggregateRootId, 1, DateTime.Now,
                                                  snpshotEvent, new Version(1, 1)));

            uncommitedSnapshotStore.Setup(x => x.GetSnapshot(aggregateRootId, 1)).Returns(targetSnpshot);

            var result = target.GetSnapshot(aggregateRootId, 1);

            Assert.IsTrue(result is CommitedSnapshot);

        }*/
    }

}
