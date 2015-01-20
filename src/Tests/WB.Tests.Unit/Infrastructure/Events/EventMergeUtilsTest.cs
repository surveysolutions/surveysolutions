using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Events;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Eventing;
using NUnit.Framework;

namespace WB.Tests.Unit.Infrastructure.Events
{
    [TestFixture]
    public class EventMergeUtilsTest
    {
        [SetUp]
        public void SetUp()
        {
            AssemblyContext.SetupServiceLocator();
        }

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
                    Guid.NewGuid(), null, Guid.NewGuid(), eventSourceGuid, 1, DateTime.Now, new object()));
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
                    Guid.NewGuid(), null, rootGuid, eventSourceGuid, 1, DateTime.Now, new object()),
                new CommittedEvent(
                    Guid.NewGuid(), null, sharedEventGuid, eventSourceGuid, 2, DateTime.Now, new object()));
            var result = stream.CreateUncommittedEventStream(commitedStream, stream[0].EventIdentifier);
            Assert.AreEqual(result.Count(), 2);
            Assert.AreEqual(result.First().EventSequence, 3);
            Assert.AreEqual(result.First().EventIdentifier, stream[1].EventIdentifier);
        }
       

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
                    Guid.NewGuid(), null, rootGuid, eventSourceGuid, 1, DateTime.Now, new object()), 
                new CommittedEvent(
                    Guid.NewGuid(), null, sharedEventGuid, eventSourceGuid, 2, DateTime.Now, new object()));
            var result = stream.CreateUncommittedEventStream(commitedStream, stream[0].EventIdentifier);
            Assert.AreEqual(result.Count(), 1);
            Assert.AreEqual(result.First().EventSequence, 3);
            Assert.AreEqual(result.First().EventIdentifier, copiedEventGuid);

            // Assert.AreEqual(result.Last().EventSequence, 3);
        }

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
                    Guid.NewGuid(), null, Guid.NewGuid(), eventSourceId, 1, DateTime.Now, new object()));
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
                    Guid.NewGuid(), null, Guid.NewGuid(), eventSourceId, 1, DateTime.Now, new object()),
                new CommittedEvent(
                    Guid.NewGuid(), null, sharedEvent, eventSourceId, 2, DateTime.Now, new object()),

                    new CommittedEvent(
                    Guid.NewGuid(), null, Guid.NewGuid(), eventSourceId, 3, DateTime.Now, new object()));
            var result = stream.FindDivergentEventGuid(baseStream);
            Assert.AreEqual(result, sharedEvent);
        }

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
                    Guid.NewGuid(), null, Guid.NewGuid(), eventSourceId, 1, DateTime.Now, new object()));
            var result = stream.FindDivergentEventGuid(baseStream);
            Assert.AreEqual(result, null);
        }

        [Test]
        public void FindDivergentSequenceNumber_BaseEventStreamChangedLEssThenNewStreamChanged_SharedEventGuidIsReturned()
        {
            Guid eventSourceId = Guid.NewGuid();
            Guid sharedEventid = Guid.NewGuid();
            var baseStream = new CommittedEventStream(
                eventSourceId, 
                new CommittedEvent(
                    Guid.NewGuid(), null, sharedEventid, eventSourceId, 1, DateTime.Now, new object()), 
                new CommittedEvent(
                    Guid.NewGuid(), null, Guid.NewGuid(), eventSourceId, 2, DateTime.Now, new object()));

            var stream = new List<AggregateRootEvent>
                {
                    new AggregateRootEvent { EventSequence = 1, Payload = new object(), EventIdentifier = sharedEventid }, 
                    new AggregateRootEvent { EventSequence = 2, Payload = new object(), EventIdentifier = Guid.NewGuid() }, 
                    new AggregateRootEvent { EventSequence = 3, Payload = new object(), EventIdentifier = Guid.NewGuid() }
                };

            var result = stream.FindDivergentEventGuid(baseStream);
            Assert.AreEqual(result, sharedEventid);
        }

        [Test]
        public void FindDivergentSequenceNumber_BaseEventStreamChangedMoreThenNewStreamChanged_SharedEventGuidIsReturned()
        {
            Guid eventSourceId = Guid.NewGuid();
            Guid sharedEventid = Guid.NewGuid();
            var baseStream = new CommittedEventStream(
                eventSourceId, 
                new CommittedEvent(
                    Guid.NewGuid(), null, sharedEventid, eventSourceId, 1, DateTime.Now, new object()), 
                new CommittedEvent(
                    Guid.NewGuid(), null, Guid.NewGuid(), eventSourceId, 2, DateTime.Now, new object()), 
                new CommittedEvent(
                    Guid.NewGuid(), null, Guid.NewGuid(), eventSourceId, 3, DateTime.Now, new object()));

            var stream = new List<AggregateRootEvent>
                {
                    new AggregateRootEvent { EventSequence = 1, Payload = new object(), EventIdentifier = sharedEventid }, 
                    new AggregateRootEvent { EventSequence = 2, Payload = new object(), EventIdentifier = Guid.NewGuid() }
                };

            var result = stream.FindDivergentEventGuid(baseStream);
            Assert.AreEqual(result, sharedEventid);
        }

        [Test]
        public void FindDivergentSequenceNumber_BaseEventStreamChangedNewStreamChanged_SharedEventGuidIsReturned()
        {
            Guid eventSourceId = Guid.NewGuid();
            Guid sharedEventid = Guid.NewGuid();
            var baseStream = new CommittedEventStream(
                eventSourceId, 
                new CommittedEvent(
                    Guid.NewGuid(), null, sharedEventid, eventSourceId, 1, DateTime.Now, new object()), 
                new CommittedEvent(
                    Guid.NewGuid(), null, Guid.NewGuid(), eventSourceId, 2, DateTime.Now, new object()));

            var stream = new List<AggregateRootEvent>
                {
                    new AggregateRootEvent { EventSequence = 1, Payload = new object(), EventIdentifier = sharedEventid }, 
                    new AggregateRootEvent { EventSequence = 2, Payload = new object(), EventIdentifier = Guid.NewGuid() }
                };

            var result = stream.FindDivergentEventGuid(baseStream);
            Assert.AreEqual(result, sharedEventid);
        }

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

        [Test]
        public void FindDivergentSequenceNumber_BaseEventStreamNotChanged_SharedEventGuidIsReturned()
        {
            Guid eventSourceId = Guid.NewGuid();
            var baseStream = new CommittedEventStream(
                eventSourceId, 
                new CommittedEvent(
                    Guid.NewGuid(), null, Guid.NewGuid(), eventSourceId, 1, DateTime.Now, new object()));

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

        [Test]
        public void FindDivergentSequenceNumber_EventStreamIsEmpty_ExeptionisThrowed()
        {
            var stream = new List<AggregateRootEvent>();
            Assert.Throws<ArgumentException>(() => stream.FindDivergentEventGuid(null));
        }
    }
}