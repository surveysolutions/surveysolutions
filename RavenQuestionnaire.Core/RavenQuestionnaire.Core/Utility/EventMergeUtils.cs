using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using RavenQuestionnaire.Core.Events;

namespace RavenQuestionnaire.Core.Utility
{
    public static class EventMergeUtils
    {
        public static long FindDivergentSequenceNumber(this IEnumerable<AggregateRootEvent> stream, CommittedEventStream baseStream)
        {
            if (!stream.Any())
                throw new ArgumentException("event stream is empty");
           // var aggregateRootId = stream.First().EventSourceId;
           // var currentStream = eventStore.ReadFrom(aggregateRootId, int.MinValue, int.MaxValue);
            if (baseStream.IsEmpty)
                return 0;
            var startPoint = Math.Min(baseStream.Last().EventSequence, stream.Last().EventSequence);
            var croppedBase = baseStream.TakeWhile(e => e.EventSequence <= startPoint);
            var croppedNewStream = stream.TakeWhile(e => e.EventSequence <= startPoint);
            while ((croppedBase.Last().EventIdentifier != croppedNewStream.Last().EventIdentifier))
            {
                startPoint--;
                croppedBase = baseStream.TakeWhile(e => e.EventSequence <= startPoint);
                croppedNewStream = stream.TakeWhile(e => e.EventSequence <= startPoint);
            }

            return startPoint;
        }
        public static UncommittedEventStream CreateUncommittedEventStream(this IEnumerable<AggregateRootEvent> stream, CommittedEventStream baseStream)
        {
            if (!stream.Any())
                throw new ArgumentException("EventSequence is empty");
             var dvergentPoint = stream.FindDivergentSequenceNumber(baseStream);
            long sequenceNumber = 0;
            if(baseStream.Any())
                sequenceNumber = baseStream.Last().EventSequence;
            var uncommitedStream = CreateUncommittedEventStream(
                stream.SkipWhile(e => e.EventSequence <= dvergentPoint), sequenceNumber);
            return uncommitedStream;
        }
        public static UncommittedEventStream CreateUncommittedEventStream(this IEnumerable<AggregateRootEvent> stream, long lastSequenceNumber)
        {
            var uncommitedStream = new UncommittedEventStream(Guid.NewGuid());

            if (!stream.Any())
                return uncommitedStream;
            long sequenceNumber = lastSequenceNumber + 1;
            foreach (AggregateRootEvent aggregateRootEvent in stream)
            {
                uncommitedStream.Append(aggregateRootEvent.CreateUncommitedEvent(sequenceNumber, 0));
                sequenceNumber++;
            }
            return uncommitedStream;
        }
    }
}
