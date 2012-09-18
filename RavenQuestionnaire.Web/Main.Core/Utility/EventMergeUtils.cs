// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EventMergeUtils.cs" company="">
//   
// </copyright>
// <summary>
//   The event merge utils.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Events;

    using Ncqrs.Eventing;

    /// <summary>
    /// The event merge utils.
    /// </summary>
    public static class EventMergeUtils
    {
        #region Public Methods and Operators

        /// <summary>
        /// The create uncommitted event stream.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <param name="baseStream">
        /// The base stream.
        /// </param>
        /// <returns>
        /// The Ncqrs.Eventing.UncommittedEventStream.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// </exception>
        public static UncommittedEventStream CreateUncommittedEventStream(
            this IEnumerable<AggregateRootEvent> stream, CommittedEventStream baseStream)
        {
            if (!stream.Any())
            {
                throw new ArgumentException("EventSequence is empty");
            }

            long dvergentPoint = stream.FindDivergentSequenceNumber(baseStream);

            UncommittedEventStream uncommitedStream = CreateUncommittedEventStream(stream, baseStream, dvergentPoint);
            return uncommitedStream;
        }

        /// <summary>
        /// The create uncommitted event stream.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <param name="baseStream">
        /// The base stream.
        /// </param>
        /// <param name="dvergentPoint">
        /// The dvergent point.
        /// </param>
        /// <returns>
        /// The Ncqrs.Eventing.UncommittedEventStream.
        /// </returns>
        public static UncommittedEventStream CreateUncommittedEventStream(
            this IEnumerable<AggregateRootEvent> stream, CommittedEventStream baseStream, long dvergentPoint)
        {
            var uncommitedStream = new UncommittedEventStream(Guid.NewGuid());

            if (!stream.Any())
            {
                return uncommitedStream;
            }

            long lastSequenceNumber = 1;
            if (baseStream.Any())
            {
                lastSequenceNumber = baseStream.Last().EventSequence + 1;
            }

            IEnumerable<AggregateRootEvent> mergedStream =
                stream.SkipWhile(e => e.EventSequence <= dvergentPoint).Where(
                    e =>
                    !baseStream.SkipWhile(c => c.EventSequence <= dvergentPoint).Any(
                        c => c.EventIdentifier == e.EventIdentifier));

            /*.Join(
                    baseStream.SkipWhile(e => e.EventSequence <= dvergentPoint),
                    (e) => e.EventIdentifier, e => e.EventIdentifier, (e, u) => e);*/
            // long sequenceNumber = lastSequenceNumber + 1;
            foreach (AggregateRootEvent aggregateRootEvent in mergedStream)
            {
                uncommitedStream.Append(aggregateRootEvent.CreateUncommitedEvent(lastSequenceNumber, 0));
                lastSequenceNumber++;
            }

            return uncommitedStream;
        }

        /// <summary>
        /// The find divergent sequence number.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <param name="baseStream">
        /// The base stream.
        /// </param>
        /// <returns>
        /// The System.Int64.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// </exception>
        public static long FindDivergentSequenceNumber(
            this IEnumerable<AggregateRootEvent> stream, CommittedEventStream baseStream)
        {
            if (!stream.Any())
            {
                throw new ArgumentException("event stream is empty");
            }

            // var aggregateRootId = stream.First().EventSourceId;
            // var currentStream = eventStore.ReadFrom(aggregateRootId, int.MinValue, int.MaxValue);
            if (baseStream.IsEmpty)
            {
                return 0;
            }

            long startPoint = Math.Min(baseStream.Last().EventSequence, stream.Last().EventSequence);
            IEnumerable<CommittedEvent> croppedBase = baseStream.TakeWhile(e => e.EventSequence <= startPoint);
            IEnumerable<AggregateRootEvent> croppedNewStream = stream.TakeWhile(e => e.EventSequence <= startPoint);

            if (!croppedNewStream.Any())
                return baseStream.Last().EventSequence;
            while (startPoint > 0 && (croppedBase.Last().EventIdentifier != croppedNewStream.Last().EventIdentifier))
            {
                startPoint--;
                croppedBase = baseStream.TakeWhile(e => e.EventSequence <= startPoint);
                croppedNewStream = stream.TakeWhile(e => e.EventSequence <= startPoint);
            }

            return startPoint;
        }

        #endregion
    }
}