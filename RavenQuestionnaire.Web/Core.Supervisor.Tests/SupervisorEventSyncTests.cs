using System;
using System.Linq;
using System.Collections.Generic;
using Core.Supervisor.Synchronization;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using Main.Core.View.CompleteQuestionnaire;
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
            denormalizerMock.Setup(x => x.Query(It.IsAny<Func<IQueryable<FileDescription>, List<Guid>>>()))
                          .Returns(new List<Guid> { });
            commandServiceMock=new Mock<ICommandService>();
        //    NcqrsEnvironment.SetDefault(eventStoreMock.Object);
            NcqrsEnvironment.SetDefault(commandServiceMock.Object);
        }
        protected Mock<IEventStore> PerpairSimpleEventStore()
        {
            var eventStoreMock = new Mock<IEventStore>();
            NcqrsEnvironment.SetDefault(eventStoreMock.Object);
            eventStoreMock.Setup(x => x.ReadFrom(It.IsAny<Guid>(),
                                                 It.IsAny<long>(), It.IsAny<long>()))
                          .Returns<Guid, long, long>(ReturnEmptyEventStream);
            return eventStoreMock;
        }

        private CommittedEventStream ReturnEmptyEventStream(Guid t, long max, long min)
        {
            return 
                   new CommittedEventStream(t);
        }

        protected Mock<ISnapshootEventStore> PrepareSnapshotableEventStore()
        {
            var eventStoreMock = new Mock<ISnapshootEventStore>();
            NcqrsEnvironment.SetDefault<IEventStore>(eventStoreMock.Object);
            var commitedEvent = new CommittedEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1, DateTime.Now,
                                                   new object(), new Version(1, 1, 1, 1));
            eventStoreMock.Setup(x => x.GetLatestSnapshoot(It.IsAny<Guid>()))
                          .Returns(commitedEvent);
            denormalizerMock.Setup(x => x.GetByGuid<CommittedEvent>(It.IsAny<Guid>())).Returns(commitedEvent);
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
            var userId = Guid.Parse("11111111111111111111111111111111");
            var lastEventId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");

            streamableEventStore.Setup(x => x.GetLastEvent(userId))
                                .Returns(lastEventId);

            denormalizerMock.Setup(x => x.Query(It.IsAny<Func<IQueryable<UserDocument>, List<Guid>>>()))
                            .Returns(new List<Guid> { userId });

          

            denormalizerMock.Setup(x => x.Query(It.IsAny<Func<IQueryable<CompleteQuestionnaireBrowseItem>, List<Guid>>>()))
                            .Returns(new List<Guid> { });

            SupervisorEventStreamReader unitUnderTest =CreateNewStreamReaderWhichIsSendApproved();


            // act
            var result =unitUnderTest.GetAllARIds().ToList();

            // assert
            Assert.That(result[0].AggregateRootPeak, Is.EqualTo(lastEventId));
            Assert.That(result[0].AggregateRootId, Is.EqualTo(userId));
        }

        private SupervisorEventStreamReader CreateNewStreamReaderWhichIsSendApproved(Guid? supervisorId=null)
        {
            return new SupervisorEventStreamReader(denormalizerMock.Object, supervisorId??Guid.NewGuid(), true);
        }
        private SupervisorEventStreamReader CreateNewStreamReaderWhichIsNotSendApproved(Guid? supervisorId = null)
        {
            return new SupervisorEventStreamReader(denormalizerMock.Object, supervisorId ?? Guid.NewGuid(), true);
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

        

            denormalizerMock.Setup(x => x.Query(It.IsAny<Func<IQueryable<CompleteQuestionnaireBrowseItem>, List<Guid>>>()))
                            .Returns(new List<Guid> { });

            SupervisorEventStreamReader unitUnderTest = CreateNewStreamReaderWhichIsSendApproved(supervisorId);


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
            SupervisorEventStreamReader target = CreateNewStreamReaderWhichIsSendApproved();
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

            SupervisorEventStreamReader target = CreateNewStreamReaderWhichIsSendApproved();
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

            SupervisorEventStreamReader target = CreateNewStreamReaderWhichIsSendApproved();
            var result = target.GetEventStreamById<DummyAR>(aggregateRootId);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].EventIdentifier == eventId);

        }
        [Test]
        public void GetEventStreamById_LastEventIsSnapshootLoaded_ReadFromWasntCalled()
        {
            var eventStoreMock = PrepareSnapshotableEventStore();
            var aggregateRootId = Guid.NewGuid();
            SupervisorEventStreamReader target = CreateNewStreamReaderWhichIsSendApproved();
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
            SupervisorEventStreamReader target = CreateNewStreamReaderWhichIsSendApproved();
            var result = target.GetEventStreamById<DummySnapshotableAR>(aggregateRootId);
            commandServiceMock.Verify(x => x.Execute(It.IsAny<CreateSnapshotForAR>()), Times.Once());

        }

        [Test]
        public void GetAllARIds_When_approved_alloved_for_synck_condition_Then_approved_questionnarie_id_is_presented()
        {
            // arrange
            var questionnarieId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var avalibleUsers = new List<Guid> { userId };
            var avalibleQuestionnaries = new List<Guid> { questionnarieId };
            denormalizerMock.Setup(x => x.Query(It.IsAny<Func<IQueryable<UserDocument>, List<Guid>>>()))
                            .Returns(avalibleUsers);

            denormalizerMock.Setup(x => x.Query(It.IsAny<Func<IQueryable<CompleteQuestionnaireBrowseItem>, List<Guid>>>()))
                            .Returns(avalibleQuestionnaries);

            var target = CreateNewStreamReaderWhichIsSendApproved();

            // act
            var avalibleRoots=target.GetAllARIds();

            // assert
            Assert.That(avalibleRoots.Count(), Is.EqualTo(2));
            Assert.That(avalibleRoots.First().AggregateRootId, Is.EqualTo(userId));
            Assert.That(avalibleRoots.Last().AggregateRootId, Is.EqualTo(questionnarieId));
        }

        [Test]
        public void GetAllARIds_When_approved_is_not_alloved_for_synck_condition_Then_approved_questionnarie_id_is_not_presented()
        {
            // arrange
            var questionnarieId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var avalibleUsers = new List<Guid> { userId };

            var avalibleQuestionnaries = new List<Guid>();
            denormalizerMock.Setup(x => x.Query(It.IsAny<Func<IQueryable<UserDocument>, List<Guid>>>()))
                           .Returns(avalibleUsers);

            denormalizerMock.Setup(x => x.Query(It.IsAny<Func<IQueryable<CompleteQuestionnaireBrowseItem>, List<Guid>>>()))
                          .Returns(avalibleQuestionnaries);

            var target = CreateNewStreamReaderWhichIsNotSendApproved();

            // act
            var avalibleRoots = target.GetAllARIds();

            // assert
            Assert.That(avalibleRoots.Count(), Is.EqualTo(1));
            Assert.That(avalibleRoots.First().AggregateRootId, Is.EqualTo(userId));        }
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