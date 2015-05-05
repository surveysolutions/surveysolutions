using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Events;

namespace Ncqrs.Eventing
{
    public static class EventMergeUtils
    {
        public static UncommittedEventStream CreateUncommittedEventStream(
            this IEnumerable<AggregateRootEvent> stream, CommittedEventStream baseStream)
        {
            if (!stream.Any())
            {
                throw new ArgumentException("EventSequence is empty");
            }

            stream = stream.OrderBy(e => e.EventSequence);
            Guid? devergentGuid = stream.FindDivergentEventGuid(baseStream);

            UncommittedEventStream uncommitedStream = CreateUncommittedEventStream(stream, baseStream, devergentGuid);
            return uncommitedStream;
        }

        public static UncommittedEventStream CreateUncommittedEventStream(
            this IEnumerable<AggregateRootEvent> stream, CommittedEventStream baseStream, Guid? dvergentEventId)
        {
            var uncommitedStream = new UncommittedEventStream(Guid.NewGuid(), null);

            if (!stream.Any())
            {
                return uncommitedStream;
            }

            int lastSequenceNumber = 1;
            if (baseStream.Any())
            {
                lastSequenceNumber = baseStream.Last().EventSequence + 1;
            }

            IEnumerable<AggregateRootEvent> mergedStream;
            if (dvergentEventId.HasValue)
            {
                mergedStream =
                    stream.SkipWhile(e => e.EventIdentifier != dvergentEventId).Where(
                        e =>
                        baseStream.SkipWhile(c => c.EventIdentifier != dvergentEventId).All(
                            c => c.EventIdentifier != e.EventIdentifier));
            }
            else
            {
                mergedStream = stream;
            }

            foreach (AggregateRootEvent aggregateRootEvent in mergedStream)
            {
                uncommitedStream.Append(aggregateRootEvent.CreateUncommitedEvent(lastSequenceNumber, 0));
                lastSequenceNumber++;
            }

            return uncommitedStream;
        }

        public static Guid? FindDivergentEventGuid(
            this IEnumerable<AggregateRootEvent> stream, CommittedEventStream baseStream)
        {
            if (!stream.Any())
            {
                throw new ArgumentException("event stream is empty");
            }
            
            if (baseStream.IsEmpty)
            {
                return null;
            }

            IEnumerable<CommittedEvent> croppedBase = baseStream;
            IEnumerable<AggregateRootEvent> croppedNewStream = stream;

            // if basestream and remote stream aren't started  from the same event
            if (baseStream.First().EventIdentifier != stream.First().EventIdentifier)
            {
                // if base stream and remote stream aren't crossed at all
                if (!baseStream.Any(e => e.EventIdentifier == stream.First().EventIdentifier))
                {
                    return null;
                }

                // make base stream and remote stream starting from same event
                croppedBase = baseStream.SkipWhile(e => e.EventIdentifier != stream.First().EventIdentifier);
            }

            long startPoint = Math.Min(croppedBase.Count(), croppedNewStream.Count());
            while (true)
            {
                croppedBase = croppedBase.TakeWhile((e, i) => i < startPoint);
                croppedNewStream = croppedNewStream.TakeWhile((e, i) => i < startPoint);
                if (!croppedNewStream.Any())
                {
                    return baseStream.Last().EventIdentifier;
                }

                if (croppedBase.Last().EventIdentifier == croppedNewStream.Last().EventIdentifier)
                {
                    return croppedBase.Last().EventIdentifier;
                }

                startPoint--;
                if (startPoint == 0)
                {
                    throw new InvalidOperationException("that is impossible they have to cross at least at first event");
                }
            }
        }
    }
}