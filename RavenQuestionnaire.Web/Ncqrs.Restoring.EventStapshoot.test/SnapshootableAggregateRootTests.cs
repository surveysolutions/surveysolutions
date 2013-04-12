// -----------------------------------------------------------------------
// <copyright file="SnapshootableAggregateRootTests.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using NUnit.Framework;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Spec;

namespace Ncqrs.Restoring.EventStapshoot.test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [TestFixture]
    public class SnapshootableAggregateRootTests
    {
        private EventContext eventContext;

        [SetUp]
        public void Init()
        {
            this.eventContext = new EventContext();
        }

        [TearDown]
        public void Dispose()
        {
            this.eventContext.Dispose();
            this.eventContext = null;
        }
        [Test]
        public void InitializeFromHistory_SnapsotsAreAbsent_RestoredFromScratch()
        {
            Guid aggreagateRootId = Guid.NewGuid();
            CommittedEventStream eventStream = new CommittedEventStream(aggreagateRootId,
                                                                        new CommittedEvent(Guid.NewGuid(),
                                                                                           Guid.NewGuid(),
                                                                                           aggreagateRootId, 1,
                                                                                           DateTime.Now,
                                                                                           new object(),
                                                                                           new Version()));
            DummyAR aggregateRoot =  CreateDummyAr();
            aggregateRoot.InitializeFromHistory(eventStream);
            Assert.IsTrue(aggregateRoot.EventHandlingCounter == 1);
        }
        /*     [Test]
             public void InitializeFromHistory_OneSnatshotIsAvalible_ExceptionThrowed()
             {
                 Guid aggreagateRootId = Guid.NewGuid();
                 CommittedEventStream eventStream = new CommittedEventStream(aggreagateRootId,
                                                                             new CommittedEvent(Guid.NewGuid(),
                                                                                                Guid.NewGuid(),
                                                                                                aggreagateRootId, 1,
                                                                                                DateTime.Now,
                                                                                                new object(),
                                                                                                new Version())
                                                                             ,
                                                                             new CommittedEvent(Guid.NewGuid(),
                                                                                                Guid.NewGuid(),
                                                                                                aggreagateRootId, 2,
                                                                                                DateTime.Now,
                                                                                                new SnapshootLoaded()
                                                                                                    {
                                                                                                        Template =
                                                                                                            new Snapshot(
                                                                                                            aggreagateRootId,
                                                                                                            2, new object())
                                                                                                    },
                                                                                                new Version())
                                                                             ,
                                                                             new CommittedEvent(Guid.NewGuid(),
                                                                                                Guid.NewGuid(),
                                                                                                aggreagateRootId, 3,
                                                                                                DateTime.Now,
                                                                                                new object(),
                                                                                                new Version()));
                 DummyAR aggregateRoot = CreateDummyAr();
                 Assert.Throws<InvalidCommittedEventException>(() => aggregateRoot.InitializeFromHistory(eventStream));
             }*/

        [Test]
        public void CreateNewSnapshot_LastEventIsUncommitedSnapshot_SnapshotSaved()
        {
            // arrange
            DummyAR ar = CreateDummyAr();
            var snapshot = new Snapshot(ar.EventSourceId, 1, new object());
            ar.InitializeFromSnapshot(snapshot);
            ar.RestoreFromSnapshot(snapshot);
            ar.CreateNewSnapshot();
            Assert.That(this.eventContext.Events.Count(), Is.EqualTo(1));
            Assert.AreEqual(ar.RestoreFromSnapshotCounter, 2);
        }
        [Test]
        public void CreateNewSnapshot_LastEventIsUncommitedSnapshot_Nothing()
        {
            // arrange
            DummyAR ar = CreateDummyAr();
            var snapshot = new CommitedSnapshot(ar.EventSourceId, 1, new object());
            ar.InitializeFromSnapshot(snapshot);
            ar.RestoreFromSnapshot(snapshot);
            ar.CreateNewSnapshot();
            Assert.That(this.eventContext.Events.Count(), Is.EqualTo(0));
            Assert.AreEqual(ar.RestoreFromSnapshotCounter, 1);
        }
        [Test]
        public void CreateNewSnapshot_SnpshotEventSequence_NewSnpapshotIsCreate()
        {
            // arrange
            DummyAR ar = CreateDummyAr();
            var snapshot = new Snapshot(ar.EventSourceId, 1, new object());
            ar.InitializeFromSnapshot(snapshot);
            ar.RestoreFromSnapshot(snapshot);
            ar.DummyCommand();
            ar.CreateNewSnapshot();
            var snapshotEvent = this.eventContext.Events.Last();
            Assert.AreEqual(snapshotEvent.EventSequence, 3);
            Assert.IsTrue(snapshotEvent.Payload is SnapshootLoaded);
        }

        [Test]
        public void CreateNewSnapshot_When_ArgumentIsNotNull_Then_ARStateIsRestoredTottalyFromMethodArgument()
        {
            // arrange
            DummyAR ar = CreateDummyAr();
            var newState = new object();

            // act
            ar.CreateNewSnapshot(newState);

            // assert
            var currentSnapshot = ar.CreateSnapshot();
            Assert.That(currentSnapshot, Is.EqualTo(newState));
        }

        [Test]
        public void CreateNewSnapshot_LastEventIsNotSnapshot_SnapshotIsCreatedAndStored()
        {
            // arrange
            DummyAR ar = CreateDummyAr();
            ar.InitializeFromHistory(new CommittedEventStream(ar.EventSourceId,
                                                              new CommittedEvent(Guid.NewGuid(), Guid.NewGuid(),
                                                                                 ar.EventSourceId, 1, DateTime.Now,
                                                                                 new object(),
                                                                                 new Version(1, 1))));
            ar.CreateNewSnapshot();
            var snapshotEvent = this.eventContext.Events.Last();
            Assert.AreEqual(snapshotEvent.EventSequence, 2);
            Assert.IsTrue(snapshotEvent.Payload is SnapshootLoaded);
            Assert.AreEqual(ar.RestoreFromSnapshotCounter, 1);
        }

        private static DummyAR CreateDummyAr()
        {
            return new DummyAR();
        }

        [Test]
        public void CreateNewSnapshot_LastEventIsNotSnapshot_EventsHasTheSameUtsTime()
        {
            DummyAR ar = CreateDummyAr();
            var eventId = Guid.NewGuid();
            ar.InitializeFromHistory(new CommittedEventStream(ar.EventSourceId, new CommittedEvent(Guid.NewGuid(),
                                                                                                 eventId,
                                                                                                 ar.EventSourceId, 1,
                                                                                                 DateTime.Now,
                                                                                                 new object(),
                                                                                                 new Version())));

            ar.CreateNewSnapshot();
            var result = this.eventContext.Events;
            Assert.IsTrue(result.Count() == 1);
            var offset = TimeZoneInfo.Utc.GetUtcOffset(result.First().EventTimeStamp);
            Assert.IsTrue(offset.Ticks == 0);

        }
        class DummyAR : SnapshootableAggregateRoot<object>
        {

            public int EventHandlingCounter { get; private set; }
            public int RestoreFromSnapshotCounter { get; private set; }
            private object state=new object();
            protected void OnDummyEventHandler(object evt)
            {
                this.EventHandlingCounter++;
            }

            public void DummyCommand()
            {
                ApplyEvent(new object());
            }

            #region Overrides of SnapshootableAggregateRoot<object>

            public override object CreateSnapshot()
            {
                return state;
            }

            public override void RestoreFromSnapshot(object snapshot)
            {
                state = snapshot;
                this.RestoreFromSnapshotCounter++;
            }

            #endregion
        }
    }

}
