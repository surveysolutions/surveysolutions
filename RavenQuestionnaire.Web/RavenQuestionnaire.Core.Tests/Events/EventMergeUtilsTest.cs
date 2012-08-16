using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using RavenQuestionnaire.Core.Events;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Tests.Events
{
    [TestFixture]
    public class EventMergeUtilsTest
    {
        #region CreateUncommittedEventStream

        
        [Test]
        public void CreateUncommittedEventStream_EventStreamWith2EventsStrartPointIs0_AllEventsAreCopied()
        {
            var stream = new List<AggregateRootEvent>
                             {
                                 new AggregateRootEvent() {EventSequence = 1, Payload = new object()},
                                 new AggregateRootEvent() {EventSequence = 2, Payload = new object()}
                             };
            var result = stream.CreateUncommittedEventStream(0);
            Assert.AreEqual(result.Count(), 2);

            Assert.AreEqual(result.First().EventSequence, 1);
            Assert.AreEqual(result.Last().EventSequence, 2);
        }
        [Test]
        public void CreateUncommittedEventStream_EventStreamWith2EventsStrartPointIs1_ZeroEventAreCopied()
        {
            var eventSourceGuid = Guid.NewGuid();
            var sharedEventGuid = Guid.NewGuid();
            var stream = new List<AggregateRootEvent>
                             {
                                 new AggregateRootEvent() {EventSequence = 1, Payload = new object(),EventIdentifier = sharedEventGuid, EventSourceId = eventSourceGuid},
                                 new AggregateRootEvent() {EventSequence = 2, Payload = new object(), EventSourceId = eventSourceGuid}
                             };
            
            var result =
                stream.CreateUncommittedEventStream(1);
            Assert.AreEqual(result.Count(), 2);
            Assert.AreEqual(result.First().EventSequence, 2);
            Assert.AreEqual(result.Last().EventSequence, 3);
        }

        [Test]
        public void CreateUncommittedEventStream_EventStreamWith2EventsStrartPointIsVeryBig_ZeroEventAreCopied()
        {
            var stream = new List<AggregateRootEvent>
                             {
                                 new AggregateRootEvent() {EventSequence = 1, Payload = new object()},
                                 new AggregateRootEvent() {EventSequence = 2, Payload = new object()}
                             };
            var result = stream.CreateUncommittedEventStream(100500);
            Assert.AreEqual(result.Count(), 2);
            Assert.AreEqual(result.First().EventSequence, 100501);
            Assert.AreEqual(result.Last().EventSequence, 100502);
        }

        #endregion

        #region FindDivergentSequenceNumber

        [Test]
        public void FindDivergentSequenceNumber_EventStreamIsEmpty_ExeptionisThrowed()
        {
            var stream = new List<AggregateRootEvent>();
            Assert.Throws<ArgumentException>(() => stream.FindDivergentSequenceNumber(null));
        }
        [Test]
        public void FindDivergentSequenceNumber_BaseEventStreamIsEmpty_ZeroReturned()
        {
            var stream = new List<AggregateRootEvent> { new AggregateRootEvent() { EventSequence = 1, Payload = new object() } };
            var baseStream = new CommittedEventStream(Guid.NewGuid());
            var result = stream.FindDivergentSequenceNumber(baseStream);
            Assert.AreEqual(result, 0);
        }

        [Test]
        public void FindDivergentSequenceNumber_BaseEventStreamNotChanged_1Returned()
        {
            var eventSourceId = Guid.NewGuid();
            var baseStream =
                new CommittedEventStream(eventSourceId,
                                         new CommittedEvent(Guid.NewGuid(), Guid.NewGuid(), eventSourceId, 1,
                                                            DateTime.Now, new object(), new Version()));

            var stream = new List<AggregateRootEvent>
                             {
                                 new AggregateRootEvent()
                                     {
                                         EventSequence = 1,
                                         Payload = new object(),
                                         EventIdentifier = baseStream.First().EventIdentifier
                                     },
                                        new AggregateRootEvent()
                                     {
                                         EventSequence = 2,
                                         Payload = new object()
                                     }
                             };

            var result = stream.FindDivergentSequenceNumber(baseStream);
            Assert.AreEqual(result, 1);
            
        }

        [Test]
        public void FindDivergentSequenceNumber_BaseEventStreamChangedNewStreamChanged_1Returned()
        {
            var eventSourceId = Guid.NewGuid();
            var sharedEventid = Guid.NewGuid();
            var baseStream =
                new CommittedEventStream(eventSourceId,
                                         new CommittedEvent(Guid.NewGuid(), sharedEventid, eventSourceId, 1,
                                                            DateTime.Now, new object(), new Version()),
                                                            new CommittedEvent(Guid.NewGuid(), Guid.NewGuid(), eventSourceId, 2,
                                                            DateTime.Now, new object(), new Version()));

            var stream = new List<AggregateRootEvent>
                             {
                                 new AggregateRootEvent()
                                     {
                                         EventSequence = 1,
                                         Payload = new object(),
                                         EventIdentifier = sharedEventid
                                     },
                                        new AggregateRootEvent()
                                     {
                                         EventSequence = 2,
                                         Payload = new object(),
                                         EventIdentifier = Guid.NewGuid()
                                     }
                             };

            var result = stream.FindDivergentSequenceNumber(baseStream);
            Assert.AreEqual(result, 1);

        }

        [Test]
        public void FindDivergentSequenceNumber_BaseEventStreamChangedMoreThenNewStreamChanged_1Returned()
        {
            var eventSourceId = Guid.NewGuid();
            var sharedEventid = Guid.NewGuid();
            var baseStream =
                new CommittedEventStream(eventSourceId,
                                         new CommittedEvent(Guid.NewGuid(), sharedEventid, eventSourceId, 1,
                                                            DateTime.Now, new object(), new Version()),
                                                            new CommittedEvent(Guid.NewGuid(), Guid.NewGuid(), eventSourceId, 2,
                                                            DateTime.Now, new object(), new Version()),
                                                            new CommittedEvent(Guid.NewGuid(), Guid.NewGuid(), eventSourceId, 3,
                                                            DateTime.Now, new object(), new Version()));

            var stream = new List<AggregateRootEvent>
                             {
                                 new AggregateRootEvent()
                                     {
                                         EventSequence = 1,
                                         Payload = new object(),
                                         EventIdentifier = sharedEventid
                                     },
                                        new AggregateRootEvent()
                                     {
                                         EventSequence = 2,
                                         Payload = new object(),
                                         EventIdentifier = Guid.NewGuid()
                                     }
                             };

            var result = stream.FindDivergentSequenceNumber(baseStream);
            Assert.AreEqual(result, 1);

        }

        [Test]
        public void FindDivergentSequenceNumber_BaseEventStreamChangedLEssThenNewStreamChanged_1Returned()
        {
            var eventSourceId = Guid.NewGuid();
            var sharedEventid = Guid.NewGuid();
            var baseStream =
                new CommittedEventStream(eventSourceId,
                                         new CommittedEvent(Guid.NewGuid(), sharedEventid, eventSourceId, 1,
                                                            DateTime.Now, new object(), new Version()),
                                                            new CommittedEvent(Guid.NewGuid(), Guid.NewGuid(), eventSourceId, 2,
                                                            DateTime.Now, new object(), new Version()));

            var stream = new List<AggregateRootEvent>
                             {
                                 new AggregateRootEvent()
                                     {
                                         EventSequence = 1,
                                         Payload = new object(),
                                         EventIdentifier = sharedEventid
                                     },
                                        new AggregateRootEvent()
                                     {
                                         EventSequence = 2,
                                         Payload = new object(),
                                         EventIdentifier = Guid.NewGuid()
                                     }
                                     ,
                                        new AggregateRootEvent()
                                     {
                                         EventSequence = 3,
                                         Payload = new object(),
                                         EventIdentifier = Guid.NewGuid()
                                     }
                             };

            var result = stream.FindDivergentSequenceNumber(baseStream);
            Assert.AreEqual(result, 1);

        }
        #endregion

    }
}
