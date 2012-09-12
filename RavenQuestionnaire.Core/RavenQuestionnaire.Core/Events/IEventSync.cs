// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IEventSync.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   Defines the IEventSync type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Ncqrs;
    using Ncqrs.Eventing;
    using Ncqrs.Eventing.ServiceModel.Bus;
    using Ncqrs.Eventing.Storage;

    using RavenQuestionnaire.Core.Utility;

    /// <summary>
    /// The EventSync interface.
    /// </summary>
    public interface IEventSync
    {
        #region Public Methods and Operators

        /// <summary>
        /// Returns list of ALL events grouped by aggregate root, please use very carefully
        /// </summary>
        /// <returns>
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; RavenQuestionnaire.Core.Events.AggregateRootEvent].
        /// </returns>
        IEnumerable<AggregateRootEvent> ReadEvents();

        /// <summary>
        /// The write events.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        void WriteEvents(IEnumerable<AggregateRootEvent> stream);

        #endregion

        //// AggregateRootEventStream ReadEventStream(Guid eventSurceId);
        //// IEnumerable<AggregateRootEventStream> ReadCompleteQuestionare();
    }

    /// <summary>
    /// The event sync extensions.
    /// </summary>
    public static class EventSyncExtensions
    {
        #region Public Methods and Operators

        /// <summary>
        /// The read events by chunks.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="chunksize">
        /// The chunksize.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; System.Collections.Generic.IEnumerable`1[T -&gt; RavenQuestionnaire.Core.Events.AggregateRootEvent]].
        /// </returns>
        public static IEnumerable<IEnumerable<AggregateRootEvent>> ReadEventsByChunks(
            this IEventSync source, int chunksize = 2048)
        {
            IEnumerable<AggregateRootEvent> events = source.ReadEvents();

            return events.Chunk(chunksize, (e, previous) => e.CommitId == previous.CommitId);
        }

        #endregion
    }

    /// <summary>
    /// The abstract event sync.
    /// </summary>
    public abstract class AbstractEventSync : IEventSync
    {
        #region Fields

        /// <summary>
        /// The event store.
        /// </summary>
        private readonly IEventStore eventStore;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractEventSync"/> class.
        /// </summary>
        /// <exception cref="Exception">
        /// </exception>
        public AbstractEventSync()
        {
            this.eventStore = NcqrsEnvironment.Get<IEventStore>();
            if (this.eventStore == null)
            {
                throw new Exception("IEventStore is not properly initialized.");
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The read events.
        /// </summary>
        /// <returns>
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; RavenQuestionnaire.Core.Events.AggregateRootEvent].
        /// </returns>
        public abstract IEnumerable<AggregateRootEvent> ReadEvents();

        /// <summary>
        /// The write events.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <exception cref="Exception">
        /// </exception>
        public void WriteEvents(IEnumerable<AggregateRootEvent> stream)
        {
            IEnumerable<UncommittedEventStream> uncommitedStreams = this.BuildEventStreams(stream);

            UncommittedEventStream[] uncommittedEventStreams = uncommitedStreams as UncommittedEventStream[]
                                                               ?? uncommitedStreams.ToArray();
            if (!uncommittedEventStreams.Any())
            {
                return;
            }

            var myEventBus = NcqrsEnvironment.Get<IEventBus>();
            if (myEventBus == null)
            {
                throw new Exception("IEventBus is not properly initialized.");
            }

            foreach (UncommittedEventStream uncommittedEventStream in uncommittedEventStreams)
            {
                if (!uncommittedEventStream.Any())
                {
                    continue;
                }

                this.eventStore.Store(uncommittedEventStream);
                myEventBus.Publish(uncommittedEventStream);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The build event streams.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; Ncqrs.Eventing.UncommittedEventStream].
        /// </returns>
        protected IEnumerable<UncommittedEventStream> BuildEventStreams(IEnumerable<AggregateRootEvent> stream)
        {
            return
                stream.GroupBy(x => x.EventSourceId).Select(
                    g => g.CreateUncommittedEventStream(this.eventStore.ReadFrom(g.Key, long.MinValue, long.MaxValue)));
        }
        protected void GetEventStreamById(List<AggregateRootEvent> retval, Guid aggregateRootId)
        {
            var events = this.eventStore.ReadFrom(aggregateRootId,
                                                     int.MinValue, int.MaxValue);
            retval.AddRange(events.Select(e => new AggregateRootEvent(e)).ToList());
        }
        #endregion
    }
}