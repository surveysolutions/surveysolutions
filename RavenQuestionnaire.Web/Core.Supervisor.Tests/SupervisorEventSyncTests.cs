using System;
using System.Linq;
using Core.Supervisor.Synchronization;
using Main.Core.View;
using Moq;
using NUnit.Framework;
using Ncqrs;
using Ncqrs.Domain;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using Ncqrs.Restoring.EventStapshoot;
using Main.DenormalizerStorage;
namespace Core.Supervisor.Tests
{
    [TestFixture]
    public class SupervisorEventSyncTests
    {
        [SetUp]
        public void Prepare()
        {
            denormalizerMock = new Mock<IDenormalizer>();
            eventStoreMock = new Mock<IEventStore>();
            eventStoreMock.Setup(x => x.ReadFrom(It.IsAny<Guid>(),
                                                 int.MinValue, int.MaxValue)).Returns(
                                                     new CommittedEventStream(Guid.NewGuid()));
            unitOfWorckFactory=new Mock<IUnitOfWorkFactory>();

            NcqrsEnvironment.SetDefault(eventStoreMock.Object);
            NcqrsEnvironment.SetDefault(unitOfWorckFactory.Object);
        }

        private Mock<IDenormalizer> denormalizerMock;
        private Mock<IEventStore> eventStoreMock;
        private Mock<IUnitOfWorkFactory> unitOfWorckFactory;
        [Test]
        public void GetEventStreamById_EventStoreIsEmpty_EmptyListIsReturned()
        {
            SupervisorEventSync target = new SupervisorEventSync(denormalizerMock.Object);
            Guid eventSourceId = Guid.NewGuid();
            var result = target.GetEventStreamById(eventSourceId, typeof(object));
            Assert.IsTrue(result.Count == 0);
            eventStoreMock.Verify(x => x.ReadFrom(eventSourceId,
                                                  int.MinValue, int.MaxValue), Times.Once());
        }
        [Test]
        public void GetEventStreamById_AggreagateRootIsNotSnapsootable_AllEventStreamIsReturned()
        {
            var aggregateRootId = Guid.NewGuid();
            var eventId = Guid.NewGuid();
            eventStoreMock.Setup(x => x.ReadFrom(aggregateRootId,
                                                 int.MinValue, int.MaxValue)).Returns(
                                                     new CommittedEventStream(aggregateRootId,
                                                                              new CommittedEvent(Guid.NewGuid(),
                                                                                                 eventId,
                                                                                                 aggregateRootId, 1,
                                                                                                 DateTime.Now,
                                                                                                 new object(),
                                                                                                 new Version())));

            SupervisorEventSync target = new SupervisorEventSync(denormalizerMock.Object);
            var result = target.GetEventStreamById(aggregateRootId, typeof(object));
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].EventIdentifier == eventId);
            
        }
        [Test]
        public void GetEventStreamById_AggreagateRootIsNotSnapshootableAggregateRoot_AllEventStreamIsReturned()
        {
            Mock<ISnapshotable<object>> aggreagateRootMock=new Mock<ISnapshotable<object>>();
            var aggregateRootId = Guid.NewGuid();
            var eventId = Guid.NewGuid();
            eventStoreMock.Setup(x => x.ReadFrom(aggregateRootId,
                                                 int.MinValue, int.MaxValue)).Returns(
                                                     new CommittedEventStream(aggregateRootId,
                                                                              new CommittedEvent(Guid.NewGuid(),
                                                                                                 eventId,
                                                                                                 aggregateRootId, 1,
                                                                                                 DateTime.Now,
                                                                                                 new object(),
                                                                                                 new Version())));

            SupervisorEventSync target = new SupervisorEventSync(denormalizerMock.Object);
            var result = target.GetEventStreamById(aggregateRootId, aggreagateRootMock.Object.GetType());
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].EventIdentifier == eventId);

        }

        [Test]
        public void GetEventStreamById_LastEventIsSnapshootLoaded_OnlyLastEventReturned()
        {
            DummyAR aggreagateRoot=new DummyAR();
            var aggregateRootId = Guid.NewGuid();
            var eventId = Guid.NewGuid();
            eventStoreMock.Setup(x => x.ReadFrom(aggregateRootId,
                                                 int.MinValue, int.MaxValue)).Returns(
                                                     new CommittedEventStream(aggregateRootId,
                                                                              new CommittedEvent(Guid.NewGuid(),
                                                                                                 Guid.NewGuid(),
                                                                                                 aggregateRootId, 1,
                                                                                                 DateTime.Now,
                                                                                                 new object(), 
                                                                                                 new Version()),
                                                                              new CommittedEvent(Guid.NewGuid(),
                                                                                                 eventId,
                                                                                                 aggregateRootId, 2,
                                                                                                 DateTime.Now,
                                                                                                 new SnapshootLoaded(),
                                                                                                 new Version())));

            SupervisorEventSync target = new SupervisorEventSync(denormalizerMock.Object);
            var result = target.GetEventStreamById(aggregateRootId, aggreagateRoot.GetType());
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].EventIdentifier == eventId);

        }
        [Test]
        public void GetEventStreamById_SnapshootBuilding_UnitOfWorkFactoryIsCalled()
        {
            DummyAR aggreagateRoot = new DummyAR();
            var aggregateRootId = Guid.NewGuid();
            var eventId = Guid.NewGuid();
            eventStoreMock.Setup(x => x.ReadFrom(aggregateRootId,
                                                 int.MinValue, int.MaxValue)).Returns(
                                                     new CommittedEventStream(aggregateRootId,
                                                                              new CommittedEvent(Guid.NewGuid(),
                                                                                                 Guid.NewGuid(),
                                                                                                 aggregateRootId, 1,
                                                                                                 DateTime.Now,
                                                                                                 new object(),
                                                                                                 new Version()),
                                                                              new CommittedEvent(Guid.NewGuid(),
                                                                                                 eventId,
                                                                                                 aggregateRootId, 2,
                                                                                                 DateTime.Now,
                                                                                                 new object(),
                                                                                                 new Version())));

            
            SupervisorEventSync target = new SupervisorEventSync(denormalizerMock.Object);
            var result = target.GetEventStreamById(aggregateRootId, aggreagateRoot.GetType());
            unitOfWorckFactory.Verify(x => x.CreateUnitOfWork(It.IsAny<Guid>()), Times.Once());

        }
        [Test]
        public void GetEventStreamById_SnapshootBuilding_UnitOfWorkGetByIdIsCalled()
        {
            DummyAR aggreagateRoot = new DummyAR();
            var aggregateRootId = Guid.NewGuid();
            var eventId = Guid.NewGuid();
            Mock<IUnitOfWorkContext> unitOfworkMock = new Mock<IUnitOfWorkContext>();
            unitOfWorckFactory.Setup(x => x.CreateUnitOfWork(It.IsAny<Guid>())).Returns(unitOfworkMock.Object);
            eventStoreMock.Setup(x => x.ReadFrom(aggregateRootId,
                                                 int.MinValue, int.MaxValue)).Returns(
                                                     new CommittedEventStream(aggregateRootId,
                                                                              new CommittedEvent(Guid.NewGuid(),
                                                                                                 Guid.NewGuid(),
                                                                                                 aggregateRootId, 1,
                                                                                                 DateTime.Now,
                                                                                                 new object(),
                                                                                                 new Version()),
                                                                              new CommittedEvent(Guid.NewGuid(),
                                                                                                 eventId,
                                                                                                 aggregateRootId, 2,
                                                                                                 DateTime.Now,
                                                                                                 new object(),
                                                                                                 new Version())));


            SupervisorEventSync target = new SupervisorEventSync(denormalizerMock.Object);
            var result = target.GetEventStreamById(aggregateRootId, aggreagateRoot.GetType());
            unitOfworkMock.Verify(x => x.GetById(typeof (DummyAR), aggregateRootId, null), Times.Once());

        }
        [Test]
        public void GetEventStreamById_SnapshootBuilding_CreateSnapshotIsCalled()
        {
            DummyAR aggreagateRoot = new DummyAR();
            var aggregateRootId = Guid.NewGuid();
            var eventId = Guid.NewGuid();
            Mock<IUnitOfWorkContext> unitOfworkMock = new Mock<IUnitOfWorkContext>();
            unitOfWorckFactory.Setup(x => x.CreateUnitOfWork(It.IsAny<Guid>())).Returns(unitOfworkMock.Object);

            unitOfworkMock.Setup(x => x.GetById(typeof (DummyAR), aggregateRootId, null)).Returns(aggreagateRoot);
            eventStoreMock.Setup(x => x.ReadFrom(aggregateRootId,
                                                 int.MinValue, int.MaxValue)).Returns(
                                                     new CommittedEventStream(aggregateRootId,
                                                                              new CommittedEvent(Guid.NewGuid(),
                                                                                                 Guid.NewGuid(),
                                                                                                 aggregateRootId, 1,
                                                                                                 DateTime.Now,
                                                                                                 new object(),
                                                                                                 new Version()),
                                                                              new CommittedEvent(Guid.NewGuid(),
                                                                                                 eventId,
                                                                                                 aggregateRootId, 2,
                                                                                                 DateTime.Now,
                                                                                                 new object(),
                                                                                                 new Version())));


            SupervisorEventSync target = new SupervisorEventSync(denormalizerMock.Object);
            var result = target.GetEventStreamById(aggregateRootId, aggreagateRoot.GetType());
            Assert.IsTrue(aggreagateRoot.IsSnapshootCreatedCalled);

        }

        [Test]
        public void GetEventStreamById_SnapshootBuilding_SnapshootLoadedEventIsStored()
        {
            DummyAR aggreagateRoot = new DummyAR();
            var aggregateRootId = Guid.NewGuid();
            var eventId = Guid.NewGuid();
            Mock<IUnitOfWorkContext> unitOfworkMock = new Mock<IUnitOfWorkContext>();
            unitOfWorckFactory.Setup(x => x.CreateUnitOfWork(It.IsAny<Guid>())).Returns(unitOfworkMock.Object);

            unitOfworkMock.Setup(x => x.GetById(typeof(DummyAR), aggregateRootId, null)).Returns(aggreagateRoot);
            eventStoreMock.Setup(x => x.ReadFrom(aggregateRootId,
                                                 int.MinValue, int.MaxValue)).Returns(
                                                     new CommittedEventStream(aggregateRootId,
                                                                              new CommittedEvent(Guid.NewGuid(),
                                                                                                 Guid.NewGuid(),
                                                                                                 aggregateRootId, 1,
                                                                                                 DateTime.Now,
                                                                                                 new object(),
                                                                                                 new Version()),
                                                                              new CommittedEvent(Guid.NewGuid(),
                                                                                                 eventId,
                                                                                                 aggregateRootId, 2,
                                                                                                 DateTime.Now,
                                                                                                 new object(),
                                                                                                 new Version())));


            SupervisorEventSync target = new SupervisorEventSync(denormalizerMock.Object);
            var result = target.GetEventStreamById(aggregateRootId, aggreagateRoot.GetType());
            eventStoreMock.Verify(x => x.Store(
                It.Is<UncommittedEventStream>((stream) => stream.Count() == 1 && stream.First().Payload is SnapshootLoaded
                )), Times.Once());

        }
        [Test]
        public void GetEventStreamById_SnapshootBuilding_SnapshootLoadedEventIsReturned()
        {
            DummyAR aggreagateRoot = new DummyAR();
            var aggregateRootId = Guid.NewGuid();
            var eventId = Guid.NewGuid();
            Mock<IUnitOfWorkContext> unitOfworkMock = new Mock<IUnitOfWorkContext>();
            unitOfWorckFactory.Setup(x => x.CreateUnitOfWork(It.IsAny<Guid>())).Returns(unitOfworkMock.Object);

            unitOfworkMock.Setup(x => x.GetById(typeof(DummyAR), aggregateRootId, null)).Returns(aggreagateRoot);
            eventStoreMock.Setup(x => x.ReadFrom(aggregateRootId,
                                                 int.MinValue, int.MaxValue)).Returns(
                                                     new CommittedEventStream(aggregateRootId,
                                                                              new CommittedEvent(Guid.NewGuid(),
                                                                                                 Guid.NewGuid(),
                                                                                                 aggregateRootId, 1,
                                                                                                 DateTime.Now,
                                                                                                 new object(),
                                                                                                 new Version()),
                                                                              new CommittedEvent(Guid.NewGuid(),
                                                                                                 eventId,
                                                                                                 aggregateRootId, 2,
                                                                                                 DateTime.Now,
                                                                                                 new object(),
                                                                                                 new Version())));


            SupervisorEventSync target = new SupervisorEventSync(denormalizerMock.Object);
            var result = target.GetEventStreamById(aggregateRootId, aggreagateRoot.GetType());
            Assert.IsTrue(result.Count==1);
            Assert.IsTrue(result[0].Payload is SnapshootLoaded);

        }
    }

    public class DummyAR : SnapshootableAggregateRoot<object>
    {
        public bool IsSnapshootCreatedCalled { get;private set; }
        public bool IsRestoreFromSnapshot { get; private set; }

        #region Overrides of SnapshootableAggregateRoot<object>

        public override object CreateSnapshot()
        {
            IsSnapshootCreatedCalled = true;
            return new object();
        }

        public override void RestoreFromSnapshot(object snapshot)
        {
            IsRestoreFromSnapshot = true;
        }

        #endregion
    }
}