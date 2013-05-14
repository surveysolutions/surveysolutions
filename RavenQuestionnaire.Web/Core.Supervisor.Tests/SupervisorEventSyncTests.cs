using System;
using System.Linq;
using System.Collections.Generic;
using Core.Supervisor.Synchronization;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using Moq;
using NUnit.Framework;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Domain;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using Ncqrs.Restoring.EventStapshoot;
using Main.DenormalizerStorage;
using Ncqrs.Restoring.EventStapshoot.EventStores;

namespace Core.Supervisor.Tests
{
    using Main.Core.View.CompleteQuestionnaire;

    [TestFixture]
    public class SupervisorEventSyncTests
    {
        [SetUp]
        public void Prepare()
        {
            denormalizerMock = new Mock<IDenormalizer>();
           // eventStoreMock = new Mock<IEventStore>();
         /*   eventStoreMock.Setup(x => x.ReadFrom(It.IsAny<Guid>(),
                                                 int.MinValue, int.MaxValue)).Returns(
                                                     new CommittedEventStream(Guid.NewGuid()));*/
            commandServiceMock=new Mock<ICommandService>();
        //    NcqrsEnvironment.SetDefault(eventStoreMock.Object);
            NcqrsEnvironment.SetDefault(commandServiceMock.Object);
        }

        protected Mock<IEventStore> PerpairSimpleEventStore()
        {
            var eventStoreMock = new Mock<IEventStore>();
            NcqrsEnvironment.SetDefault(eventStoreMock.Object);
            eventStoreMock.Setup(x => x.ReadFrom(It.IsAny<Guid>(),
                                                  int.MinValue, int.MaxValue)).Returns(
                                                      new CommittedEventStream(Guid.NewGuid()));
            return eventStoreMock;
        }

        protected Mock<ISnapshootEventStore> PrepareSnapshotableEventStore()
        {
            var eventStoreMock = new Mock<ISnapshootEventStore>();
            NcqrsEnvironment.SetDefault<IEventStore>(eventStoreMock.Object);
            eventStoreMock.Setup(x => x.GetLatestSnapshoot(It.IsAny<Guid>()))
                          .Returns(new CommittedEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1, DateTime.Now,
                                                      new object(), new Version(1,1,1,1)));
            return eventStoreMock;
        }

        private Mock<IDenormalizer> denormalizerMock;

        private Mock<ICommandService> commandServiceMock;

        [Test]
        public void GetAllARIds_When_StreamableEventStore_Then_LastEventByRootisReturned()
        {
            // arrange
            var eventStoreMock = new Mock<IEventStore>();
            var streamableEventStore = eventStoreMock.As<IStreamableEventStore>();
            NcqrsEnvironment.SetDefault(eventStoreMock.Object);
            var userId = Guid.NewGuid();
            var supervisorId = Guid.NewGuid();
            var lastEventId = Guid.NewGuid();

            streamableEventStore.Setup(x => x.GetLastEvent(userId))
                                .Returns(new CommittedEvent(Guid.NewGuid(), lastEventId, userId, 1, DateTime.Now,
                                                            new object(), new Version(1, 1)));

            denormalizerMock.Setup(x => x.Query(It.IsAny<Func<IQueryable<UserDocument>, List<Guid>>>()))
                            .Returns(new List<Guid> { userId });

            denormalizerMock.Setup(x => x.Query(It.IsAny<Func<IQueryable<FileDescription>, List<Guid>>>()))
                            .Returns(new List<Guid> { });

            denormalizerMock.Setup(x => x.Query(It.IsAny<Func<IQueryable<CompleteQuestionnaireBrowseItem>, List<Guid>>>()))
                            .Returns(new List<Guid> { });

            SupervisorEventStreamReader unitUnderTest = new SupervisorEventStreamReader(denormalizerMock.Object, supervisorId);

            // act
            var result =unitUnderTest.GetAllARIds().ToList();

            // assert
            Assert.That(result[0].AggregateRootPeak, Is.EqualTo(lastEventId));
            Assert.That(result[0].AggregateRootId, Is.EqualTo(userId));
        }

        [Test]
        public void GetAllARIds_When_NotStreamableEventStore_Then_LastEventByRootisNull()
        {
            // arrange
            var eventStoreMock = new Mock<IEventStore>();
            NcqrsEnvironment.SetDefault(eventStoreMock.Object);
            var userId = Guid.NewGuid();
            var supervisorId = Guid.NewGuid();

            denormalizerMock.Setup(x => x.Query(It.IsAny<Func<IQueryable<UserDocument>, List<Guid>>>()))
                            .Returns(new List<Guid> { userId });

            denormalizerMock.Setup(x => x.Query(It.IsAny<Func<IQueryable<FileDescription>, List<Guid>>>()))
                            .Returns(new List<Guid> { });

            denormalizerMock.Setup(x => x.Query(It.IsAny<Func<IQueryable<CompleteQuestionnaireBrowseItem>, List<Guid>>>()))
                            .Returns(new List<Guid> { });

            SupervisorEventStreamReader unitUnderTest = new SupervisorEventStreamReader(denormalizerMock.Object, supervisorId);

            // act
            var result = unitUnderTest.GetAllARIds().ToList();

            // assert
            Assert.That(result[0].AggregateRootPeak, Is.EqualTo(null));
            Assert.That(result[0].AggregateRootId, Is.EqualTo(userId));
        }
       
        [Test]
        public void GetEventStreamById_EventStoreIsEmpty_EmptyListIsReturned()
        {
            var eventStoreMock = PerpairSimpleEventStore();
            SupervisorEventStreamReader target = new SupervisorEventStreamReader(denormalizerMock.Object, Guid.NewGuid());
            Guid eventSourceId = Guid.NewGuid();
            var result = target.GetEventStreamById<DummySnapshotableAR>(eventSourceId);
            Assert.IsTrue(result.Count == 0);
            eventStoreMock.Verify(x => x.ReadFrom(eventSourceId,
                                                  int.MinValue, int.MaxValue), Times.Once());
        }
      
        [Test]
        public void GetEventStreamById_AggreagateRootIsSnapsootableEventStoreIsNotSnapshotable_AllEventStreamIsReturned()
        {
            var eventStoreMock = PerpairSimpleEventStore();
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
                                                                                                 new Version()),
                                                                                                 new CommittedEvent(Guid.NewGuid(),
                                                                                                 Guid.NewGuid(),
                                                                                                 aggregateRootId, 2,
                                                                                                 DateTime.Now,
                                                                                                 new object(),
                                                                                                 new Version())));

            SupervisorEventStreamReader target = new SupervisorEventStreamReader(denormalizerMock.Object, Guid.NewGuid());
            var result = target.GetEventStreamById<DummySnapshotableAR>(aggregateRootId);
            Assert.IsTrue(result.Count == 2);

        }
        [Test]
        public void GetEventStreamById_AggreagateRootIsNotSnapsootable_AllEventStreamIsReturned()
        {
            var eventStoreMock = PrepareSnapshotableEventStore();
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

            SupervisorEventStreamReader target = new SupervisorEventStreamReader(denormalizerMock.Object, Guid.NewGuid());
            var result = target.GetEventStreamById<DummyAR>(aggregateRootId);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].EventIdentifier == eventId);

        }
        [Test]
        public void GetEventStreamById_LastEventIsSnapshootLoaded_ReadFromWasntCalled()
        {
            var eventStoreMock = PrepareSnapshotableEventStore();
            var aggregateRootId = Guid.NewGuid();
            SupervisorEventStreamReader target = new SupervisorEventStreamReader(denormalizerMock.Object, Guid.NewGuid());
            var result = target.GetEventStreamById<DummySnapshotableAR>(aggregateRootId);
            Assert.IsTrue(result.Count == 1);
            eventStoreMock.Verify(x => x.ReadFrom(aggregateRootId, It.IsAny<long>(), It.IsAny<long>()), Times.Never());
        }

        [Test]
        public void GetEventStreamById_SnapshootBuilding_CommandForSnapshotCreatinSended()
        {
            var eventStoreMock = PrepareSnapshotableEventStore();
            DummySnapshotableAR aggreagateRoot = new DummySnapshotableAR();
            var aggregateRootId = Guid.NewGuid();
            SupervisorEventStreamReader target = new SupervisorEventStreamReader(denormalizerMock.Object, Guid.NewGuid());
            var result = target.GetEventStreamById<DummySnapshotableAR>(aggregateRootId);
            commandServiceMock.Verify(x => x.Execute(It.IsAny<CreateSnapshotForAR>()), Times.Once());

        }

      
    }

    public class DummyAR : AggregateRoot
    {
    }

    public class DummySnapshotableAR : SnapshootableAggregateRoot<object>
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