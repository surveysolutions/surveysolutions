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
            DummyAR aggregateRoot = new DummyAR();
            aggregateRoot.InitializeFromHistory(eventStream);
            Assert.IsTrue(aggregateRoot.EventHandlingCounter == 1);
        }
        [Test]
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
            DummyAR aggregateRoot = new DummyAR();
            Assert.Throws<InvalidCommittedEventException>(() => aggregateRoot.InitializeFromHistory(eventStream));
        }

        [Test]
        public void CreateNewSnapshot_LastEventIsSnapshot_Nothing()
        {
            // arrange
            DummyAR ar =new DummyAR();
            var snapshot = new Snapshot(ar.EventSourceId, 1, new object());
            ar.InitializeFromSnapshot(snapshot);
            ar.RestoreFromSnapshot(snapshot);
            ar.CreateNewSnapshot();
            Assert.That(this.eventContext.Events.Count(), Is.EqualTo(0));
            Assert.AreEqual(ar.RestoreFromSnapshotCounter, 1);
        }
        [Test]
        public void CreateNewSnapshot_LastEventIsNotSnapshot_SnapshotIsCreatedAndStored()
        {
            // arrange
            DummyAR ar = new DummyAR();
            ar.InitializeFromHistory(new CommittedEventStream(ar.EventSourceId,
                                                              new CommittedEvent(Guid.NewGuid(), Guid.NewGuid(),
                                                                                 ar.EventSourceId, 1, DateTime.Now,
                                                                                 new object(), 
                                                                                 new Version(1, 1))));
            ar.CreateNewSnapshot();
            var snapshotEvent = this.eventContext.Events.Last();
            Assert.AreEqual(snapshotEvent.EventSequence, 2);
            Assert.IsTrue(snapshotEvent.Payload is SnapshootLoaded);
            Assert.AreEqual(ar.RestoreFromSnapshotCounter, 0);
        }

        [Test]
        public void CreateNewSnapshot_LastEventIsNotSnapshot_EventsHasTheSameUtsTime()
        {
            DummyAR ar = new DummyAR();
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

         /*   eventStoreMock.Verify(
                x => x.Store(It.Is<UncommittedEventStream>(e => e.First().EventTimeStamp == result.First().EventTimeStamp)),
                Times.Once());*/

        }
        class DummyAR : SnapshootableAggregateRoot<object>
        {

            public int EventHandlingCounter { get; private set; }
            public int RestoreFromSnapshotCounter { get; private set; }
            protected void OnDummyEventHandler(object evt)
            {
                this.EventHandlingCounter++;
            }

            #region Overrides of SnapshootableAggregateRoot<object>

            public override object CreateSnapshot()
            {
                return new object();
            }

            public override void RestoreFromSnapshot(object snapshot)
            {
                this.RestoreFromSnapshotCounter++;
            }

            #endregion
        }
    }

}
