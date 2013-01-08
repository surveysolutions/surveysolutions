// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbstractSnapshotableEventStreamReader.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Ncqrs;
    using Ncqrs.Domain;
    using Ncqrs.Eventing;
    using Ncqrs.Eventing.Sourcing.Snapshotting;
    using Ncqrs.Eventing.Storage;
    using Ncqrs.Restoring.EventStapshoot;

    /// <summary>
    /// For load Snapshots.
    /// </summary>
    public abstract class AbstractSnapshotableEventStreamReader : AbstractEventStreamReader
    {
        #region Fields

        /// <summary>
        /// myEventStore object
        /// </summary>
        private readonly IEventStore myEventStore;

        /// <summary>
        /// unit of Work factory
        /// </summary>
        private readonly IUnitOfWorkFactory unitOfWorkFactory;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractSnapshotableEventStreamReader"/> class.
        /// </summary>
        /// <exception cref="Exception">
        /// exception not found event store
        /// </exception>
        protected AbstractSnapshotableEventStreamReader()
        {
            this.myEventStore = NcqrsEnvironment.Get<IEventStore>();
            if (this.myEventStore == null)
                throw new Exception("IEventStore is not correct.");

            this.unitOfWorkFactory = NcqrsEnvironment.Get<IUnitOfWorkFactory>();
        }

        #endregion

        #region PublicMethods

        /// <summary>
        /// The get event stream by id.
        /// </summary>
        /// <param name="aggregateRootId">
        /// The aggregate root id.
        /// </param>
        /// <param name="arType">
        /// The ar type.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public virtual List<AggregateRootEvent> GetEventStreamById(Guid aggregateRootId, Type arType)
        {
            var events = this.myEventStore.ReadFrom(aggregateRootId, int.MinValue, int.MaxValue);

            if (!events.Any())
            {
                return new List<AggregateRootEvent>(0);
            }

            var snapshotables = from i in arType.GetInterfaces()
                                where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISnapshotable<>)
                                select i;
            if (!snapshotables.Any())
            {
                return this.BuildEventStream(events);
            }

            if (!typeof(SnapshootableAggregateRoot<>).MakeGenericType(
                    snapshotables.FirstOrDefault().GetGenericArguments()[0]).IsAssignableFrom(arType))
            {
                return BuildEventStream(events);
            }

            if (events.Last().Payload is SnapshootLoaded)
            {
                return new List<AggregateRootEvent>()
                           {
                               new AggregateRootEvent(events.Last())
                           };
            }

            AggregateRoot aggregateRoot;
            using (var unitOfWork = this.unitOfWorkFactory.CreateUnitOfWork(Guid.NewGuid()))
            {
                if (unitOfWork == null)
                {
                    return this.BuildEventStream(events);
                }

                aggregateRoot = unitOfWork.GetById(arType, aggregateRootId, null);
                if (aggregateRoot == null)
                {
                    return this.BuildEventStream(events);
                }
            }

            var snapshoot = arType.GetMethod("CreateSnapshot").Invoke(aggregateRoot, new object[0]);
            var eventSnapshoot = new SnapshootLoaded() { Template = new Snapshot(aggregateRootId, 1, snapshoot) };
            Guid commitId = Guid.NewGuid();
            Guid eventId = Guid.NewGuid();
            var uncommitedStream = new UncommittedEventStream(commitId);
            var dateOfEvent = NcqrsEnvironment.Get<IClock>().UtcNow();

            uncommitedStream.Append(
                new UncommittedEvent(
                    eventId,
                    aggregateRootId,
                    aggregateRoot.Version + 1,
                    aggregateRoot.InitialVersion,
                    dateOfEvent,
                    eventSnapshoot,
                    events.Last().GetType().Assembly.GetName().Version));

            this.myEventStore.Store(uncommitedStream);

            return new List<AggregateRootEvent>()
                {
                    new AggregateRootEvent(
                        new CommittedEvent(
                        commitId,
                        eventId,
                        aggregateRootId,
                        1,
                        dateOfEvent,
                        eventSnapshoot,
                        events.Last().GetType().Assembly.GetName().Version))
                       };

        }

        #endregion

        #region PrivateMethods

        /// <summary>
        /// build eventstream from events
        /// </summary>
        /// <param name="events">
        /// The events.
        /// </param>
        /// <returns>
        /// events
        /// </returns>
        private List<AggregateRootEvent> BuildEventStream(IEnumerable<CommittedEvent> events)
        {
            return events.Select(e => new AggregateRootEvent(e)).ToList();
        }

        #endregion
    }
}
