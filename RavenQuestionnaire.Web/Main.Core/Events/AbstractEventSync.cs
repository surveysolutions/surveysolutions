// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbstractEventSync.cs" company="">
//   
// </copyright>
// <summary>
//   The abstract event sync.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Utility;

    using Ncqrs;
    using Ncqrs.Eventing;
    using Ncqrs.Eventing.ServiceModel.Bus;
    using Ncqrs.Eventing.Storage;

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

        //public abstract IEnumerable<AggregateRootEvent> ReadFilteredEvents(Guid syncKey);

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
            foreach (IGrouping<Guid, AggregateRootEvent> g in stream.GroupBy(x => x.EventSourceId))
            {
                yield return
                    g.CreateUncommittedEventStream(this.eventStore.ReadFrom(g.Key, long.MinValue, long.MaxValue));
            }
        }

        #endregion
    }
}