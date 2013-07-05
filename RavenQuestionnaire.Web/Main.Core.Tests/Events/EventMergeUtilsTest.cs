namespace RavenQuestionnaire.Core.Tests.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Events;
    using Main.Core.Utility;

    using Ncqrs.Eventing;

    using NUnit.Framework;

    /// <summary>
    /// The event merge utils test.
    /// </summary>
    [TestFixture]
    public class EventMergeUtilsTest
    {
        #region Public Methods and Operators

        /// <summary>
        /// The create uncommitted event stream_ event stream with 2 events strart point is 0_ all events are copied.
        /// </summary>
        [Test]
        public void CreateUncommittedEventStream_EventStreamWith2EventsStrartPointIs0_AllEventsAreCopied()
        {
            var stream = new List<AggregateRootEvent>
                {
                    new AggregateRootEvent { EventSequence = 1, Payload = new object() }, 
                    new AggregateRootEvent { EventSequence = 2, Payload = new object() }
                };
            var commitedStream = new CommittedEventStream(Guid.NewGuid());
            var result = stream.CreateUncommittedEventStream(commitedStream, null);
            Assert.AreEqual(result.Count(), 2);

            Assert.AreEqual(result.First().EventSequence, 1);
            Assert.AreEqual(result.Last().EventSequence, 2);
        }

        /// <summary>
        /// The create uncommitted event stream_ event stream with 2 events strart point is 1_2 event are copied.
        /// </summary>
        [Test]
        public void CreateUncommittedEventStream_EventStreamWith2EventsStrartPointIs1_2EventAreCopied()
        {
            Guid eventSourceGuid = Guid.NewGuid();

            // var sharedEventGuid = Guid.NewGuid();
            var stream = new List<AggregateRootEvent>
                {
                    new AggregateRootEvent
                        {
                            EventSequence = 1, 
                            Payload = new object(), 
                            EventIdentifier = Guid.NewGuid(), 
                            EventSourceId = eventSourceGuid
                        }, 
                    new AggregateRootEvent
                        {
                           EventSequence = 2, Payload = new object(), EventSourceId = eventSourceGuid 
                        }
                };
            var commitedStream = new CommittedEventStream(
                eventSourceGuid, 
                new CommittedEvent(
                    Guid.NewGuid(), Guid.NewGuid(), eventSourceGuid, 1, DateTime.Now, new object(), new Version()));
            var result = stream.CreateUncommittedEventStream(commitedStream, null);
            Assert.AreEqual(result.Count(), 2);
            Assert.AreEqual(result.First().EventSequence, 2);
            Assert.AreEqual(result.Last().EventSequence, 3);
        }

        [Test]
        public void CreateUncommittedEventStream_RemoteStreamDontHaveAllHistoryOnlyLastEventFromBaseStreamOnFirstPalce_EverithingAfterFirstEventIsCopiedInTail()
        {
            Guid eventSourceGuid = Guid.NewGuid();
            Guid rootGuid = Guid.NewGuid();
            Guid sharedEventGuid = Guid.NewGuid();
            var stream = new List<AggregateRootEvent>
                {
                    new AggregateRootEvent
                        {
                            EventSequence = 1, 
                            Payload = new object(), 
                            EventIdentifier = sharedEventGuid, 
                            EventSourceId = eventSourceGuid
                        }, 
                    new AggregateRootEvent
                        {
                            EventSequence = 2, 
                            Payload = new object(), 
                            EventIdentifier = Guid.NewGuid(), 
                            EventSourceId = eventSourceGuid
                        }, 
                    new AggregateRootEvent
                        {
                            EventSequence = 3, 
                            Payload = new object(), 
                            EventIdentifier = Guid.NewGuid(), 
                            EventSourceId = eventSourceGuid
                        }
                };
            var commitedStream = new CommittedEventStream(
                eventSourceGuid,
                new CommittedEvent(
                    Guid.NewGuid(), rootGuid, eventSourceGuid, 1, DateTime.Now, new object(), new Version()),
                new CommittedEvent(
                    Guid.NewGuid(), sharedEventGuid, eventSourceGuid, 2, DateTime.Now, new object(), new Version()));
            var result = stream.CreateUncommittedEventStream(commitedStream, stream[0].EventIdentifier);
            Assert.AreEqual(result.Count(), 2);
            Assert.AreEqual(result.First().EventSequence, 3);
            Assert.AreEqual(result.First().EventIdentifier, stream[1].EventIdentifier);
        }
       

        /// <summary>
        /// The create uncommitted event stream_ event stream with 3 events base stream with 2 events_ only 1 event is copien to tail.
        /// </summary>
        [Test]
        public void CreateUncommittedEventStream_EventStreamWith3EventsBaseStreamWith2Events_Only1EventIsCopienToTail()
        {
            Guid eventSourceGuid = Guid.NewGuid();
            Guid rootGuid = Guid.NewGuid();
            Guid sharedEventGuid = Guid.NewGuid();

            Guid copiedEventGuid = Guid.NewGuid();
            var stream = new List<AggregateRootEvent>
                {
                    new AggregateRootEvent
                        {
                            EventSequence = 1, 
                            Payload = new object(), 
                            EventIdentifier = rootGuid, 
                            EventSourceId = eventSourceGuid
                        }, 
                    new AggregateRootEvent
                        {
                            EventSequence = 2, 
                            Payload = new object(), 
                            EventIdentifier = copiedEventGuid, 
                            EventSourceId = eventSourceGuid
                        }, 
                    new AggregateRootEvent
                        {
                            EventSequence = 3, 
                            Payload = new object(), 
                            EventIdentifier = sharedEventGuid, 
                            EventSourceId = eventSourceGuid
                        }
                };
            var commitedStream = new CommittedEventStream(
                eventSourceGuid, 
                new CommittedEvent(
                    Guid.NewGuid(), rootGuid, eventSourceGuid, 1, DateTime.Now, new object(), new Version()), 
                new CommittedEvent(
                    Guid.NewGuid(), sharedEventGuid, eventSourceGuid, 2, DateTime.Now, new object(), new Version()));
            var result = stream.CreateUncommittedEventStream(commitedStream, stream[0].EventIdentifier);
            Assert.AreEqual(result.Count(), 1);
            Assert.AreEqual(result.First().EventSequence, 3);
            Assert.AreEqual(result.First().EventIdentifier, copiedEventGuid);

            // Assert.AreEqual(result.Last().EventSequence, 3);
        }

        /*  [Test]
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
        }*/
        [Test]
        public void FindDivergentSequenceNumber_RemoteStreamDontStartsFrom1_StreamsDoesntCross_NullIsReturned()
        {
            Guid eventSourceId = Guid.NewGuid();
            var stream = new List<AggregateRootEvent>
                {
                    new AggregateRootEvent { EventSequence = 50, EventIdentifier = Guid.NewGuid(), Payload = new object() }
                };

            var baseStream = new CommittedEventStream(
                eventSourceId,
                new CommittedEvent(
                    Guid.NewGuid(), Guid.NewGuid(), eventSourceId, 1, DateTime.Now, new object(), new Version()));
            var result = stream.FindDivergentEventGuid(baseStream);
            Assert.AreEqual(result, null);
        }
        [Test]
        public void FindDivergentSequenceNumber_RemoteStreamDontStartsFrom1_StreamsAreCrossed_NLastSharedEventGuidIsReturned()
        {
            Guid eventSourceId = Guid.NewGuid();
            Guid sharedEvent = Guid.NewGuid();
            var stream = new List<AggregateRootEvent>
                {
                    new AggregateRootEvent {EventSequence = 2, EventIdentifier = sharedEvent, Payload = new object()},
                    new AggregateRootEvent {EventSequence = 3, EventIdentifier = Guid.NewGuid(), Payload = new object()}

                };

            var baseStream = new CommittedEventStream(
                eventSourceId,
                  new CommittedEvent(
                    Guid.NewGuid(), Guid.NewGuid(), eventSourceId, 1, DateTime.Now, new object(), new Version()),
                new CommittedEvent(
                    Guid.NewGuid(), sharedEvent, eventSourceId, 2, DateTime.Now, new object(), new Version()),

                    new CommittedEvent(
                    Guid.NewGuid(), Guid.NewGuid(), eventSourceId, 3, DateTime.Now, new object(), new Version()));
            var result = stream.FindDivergentEventGuid(baseStream);
            Assert.AreEqual(result, sharedEvent);
        }
        /// <summary>
        /// The find divergent sequence number_ base event stream and new event contains do crossed_ zero returned.
        /// </summary>
        [Test]
        public void FindDivergentSequenceNumber_BaseEventStreamAndNewEventContainsDoNotCrossed_NullIsReturned()
        {
            Guid eventSourceId = Guid.NewGuid();
            var stream = new List<AggregateRootEvent>
                {
                    new AggregateRootEvent { EventSequence = 1, EventIdentifier = Guid.NewGuid(), Payload = new object() }
                };

            var baseStream = new CommittedEventStream(
                eventSourceId, 
                new CommittedEvent(
                    Guid.NewGuid(), Guid.NewGuid(), eventSourceId, 1, DateTime.Now, new object(), new Version()));
            var result = stream.FindDivergentEventGuid(baseStream);
            Assert.AreEqual(result, null);
        }

        /// <summary>
        /// The find divergent sequence number_ base event stream changed l ess then new stream changed_1 returned.
        /// </summary>
        [Test]
        public void FindDivergentSequenceNumber_BaseEventStreamChangedLEssThenNewStreamChanged_SharedEventGuidIsReturned()
        {
            Guid eventSourceId = Guid.NewGuid();
            Guid sharedEventid = Guid.NewGuid();
            var baseStream = new CommittedEventStream(
                eventSourceId, 
                new CommittedEvent(
                    Guid.NewGuid(), sharedEventid, eventSourceId, 1, DateTime.Now, new object(), new Version()), 
                new CommittedEvent(
                    Guid.NewGuid(), Guid.NewGuid(), eventSourceId, 2, DateTime.Now, new object(), new Version()));

            var stream = new List<AggregateRootEvent>
                {
                    new AggregateRootEvent { EventSequence = 1, Payload = new object(), EventIdentifier = sharedEventid }, 
                    new AggregateRootEvent { EventSequence = 2, Payload = new object(), EventIdentifier = Guid.NewGuid() }, 
                    new AggregateRootEvent { EventSequence = 3, Payload = new object(), EventIdentifier = Guid.NewGuid() }
                };

            var result = stream.FindDivergentEventGuid(baseStream);
            Assert.AreEqual(result, sharedEventid);
        }

        /// <summary>
        /// The find divergent sequence number_ base event stream changed more then new stream changed_1 returned.
        /// </summary>
        [Test]
        public void FindDivergentSequenceNumber_BaseEventStreamChangedMoreThenNewStreamChanged_SharedEventGuidIsReturned()
        {
            Guid eventSourceId = Guid.NewGuid();
            Guid sharedEventid = Guid.NewGuid();
            var baseStream = new CommittedEventStream(
                eventSourceId, 
                new CommittedEvent(
                    Guid.NewGuid(), sharedEventid, eventSourceId, 1, DateTime.Now, new object(), new Version()), 
                new CommittedEvent(
                    Guid.NewGuid(), Guid.NewGuid(), eventSourceId, 2, DateTime.Now, new object(), new Version()), 
                new CommittedEvent(
                    Guid.NewGuid(), Guid.NewGuid(), eventSourceId, 3, DateTime.Now, new object(), new Version()));

            var stream = new List<AggregateRootEvent>
                {
                    new AggregateRootEvent { EventSequence = 1, Payload = new object(), EventIdentifier = sharedEventid }, 
                    new AggregateRootEvent { EventSequence = 2, Payload = new object(), EventIdentifier = Guid.NewGuid() }
                };

            var result = stream.FindDivergentEventGuid(baseStream);
            Assert.AreEqual(result, sharedEventid);
        }

        /// <summary>
        /// The find divergent sequence number_ base event stream changed new stream changed_1 returned.
        /// </summary>
        [Test]
        public void FindDivergentSequenceNumber_BaseEventStreamChangedNewStreamChanged_SharedEventGuidIsReturned()
        {
            Guid eventSourceId = Guid.NewGuid();
            Guid sharedEventid = Guid.NewGuid();
            var baseStream = new CommittedEventStream(
                eventSourceId, 
                new CommittedEvent(
                    Guid.NewGuid(), sharedEventid, eventSourceId, 1, DateTime.Now, new object(), new Version()), 
                new CommittedEvent(
                    Guid.NewGuid(), Guid.NewGuid(), eventSourceId, 2, DateTime.Now, new object(), new Version()));

            var stream = new List<AggregateRootEvent>
                {
                    new AggregateRootEvent { EventSequence = 1, Payload = new object(), EventIdentifier = sharedEventid }, 
                    new AggregateRootEvent { EventSequence = 2, Payload = new object(), EventIdentifier = Guid.NewGuid() }
                };

            var result = stream.FindDivergentEventGuid(baseStream);
            Assert.AreEqual(result, sharedEventid);
        }

        /// <summary>
        /// The find divergent sequence number_ base event stream is empty_ zero returned.
        /// </summary>
        [Test]
        public void FindDivergentSequenceNumber_BaseEventStreamIsEmpty_NullIsReturned()
        {
            var stream = new List<AggregateRootEvent>
                {
                   new AggregateRootEvent { EventSequence = 1, Payload = new object() } 
                };
            var baseStream = new CommittedEventStream(Guid.NewGuid());
            var result = stream.FindDivergentEventGuid(baseStream);
            Assert.AreEqual(result, null);
        }

        /// <summary>
        /// The find divergent sequence number_ base event stream not changed_1 returned.
        /// </summary>
        [Test]
        public void FindDivergentSequenceNumber_BaseEventStreamNotChanged_SharedEventGuidIsReturned()
        {
            Guid eventSourceId = Guid.NewGuid();
            var baseStream = new CommittedEventStream(
                eventSourceId, 
                new CommittedEvent(
                    Guid.NewGuid(), Guid.NewGuid(), eventSourceId, 1, DateTime.Now, new object(), new Version()));

            var stream = new List<AggregateRootEvent>
                {
                    new AggregateRootEvent
                        {
                            EventSequence = 1, 
                            Payload = new object(), 
                            EventIdentifier = baseStream.First().EventIdentifier
                        }, 
                    new AggregateRootEvent { EventSequence = 2, Payload = new object() }
                };

            var result = stream.FindDivergentEventGuid(baseStream);
            Assert.AreEqual(result, baseStream.First().EventIdentifier);
        }

        /// <summary>
        /// The find divergent sequence number_ event stream is empty_ exeptionis throwed.
        /// </summary>
        [Test]
        public void FindDivergentSequenceNumber_EventStreamIsEmpty_ExeptionisThrowed()
        {
            var stream = new List<AggregateRootEvent>();
            Assert.Throws<ArgumentException>(() => stream.FindDivergentEventGuid(null));
        }

        #endregion
    }
}