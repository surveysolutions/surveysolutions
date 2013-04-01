// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbstractSnapshotableEventStreamReader.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Restoring.EventStapshoot.EventStores;

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

        private readonly ICommandService commandInvoker;
  /*      /// <summary>
        /// unit of Work factory
        /// </summary>
        private readonly IUnitOfWorkFactory unitOfWorkFactory;*/

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
            this.commandInvoker = NcqrsEnvironment.Get<ICommandService>();
            //    this.unitOfWorkFactory = NcqrsEnvironment.Get<IUnitOfWorkFactory>();
        }

        #endregion

        protected List<AggregateRootEvent> ReturnAllEventStream(Guid aggregateRootId)
        {
              var events = this.myEventStore.ReadFrom(aggregateRootId, int.MinValue, int.MaxValue);

            if (!events.Any())
            {
                return new List<AggregateRootEvent>(0);
            }
            return  this.BuildEventStream(events);
        }

        #region PublicMethods

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="aggregateRootId"></param>
        /// <returns></returns>
        public virtual List<AggregateRootEvent> GetEventStreamById<T>(Guid aggregateRootId) where T : AggregateRoot
        {
            var snapshotableEventStore = this.myEventStore as ISnapshootEventStore;
            if(snapshotableEventStore==null)
                return ReturnAllEventStream(aggregateRootId);
            Type arType = typeof (T);
            var snapshotables = from i in arType.GetInterfaces()
                                where i.IsGenericType && i.GetGenericTypeDefinition() == typeof (ISnapshotable<>)
                                select i;
            if (!snapshotables.Any())
            {
                return ReturnAllEventStream(aggregateRootId);
            }

            if (!typeof (SnapshootableAggregateRoot<>).MakeGenericType(
                snapshotables.FirstOrDefault().GetGenericArguments()[0]).IsAssignableFrom(arType))
            {
                return ReturnAllEventStream(aggregateRootId);
            }

            /*    if (events.Last().Payload is SnapshootLoaded)
                {
                    return new List<AggregateRootEvent>()
                               {
                                   new AggregateRootEvent(events.Last())
                               };
                }*/
            this.commandInvoker.Execute(new CreateSnapshotForAR(aggregateRootId, arType));
            var snapshotLoaded = snapshotableEventStore.GetLatestSnapshoot(aggregateRootId);
              return new List<AggregateRootEvent>()
                {
                    new AggregateRootEvent(snapshotLoaded)
                };
             
           /* T aggregateRoot;
            using (var unitOfWork = this.unitOfWorkFactory.CreateUnitOfWork(Guid.NewGuid()))
            {
                if (unitOfWork == null)
                {
                    return ReturnAllEventStream(aggregateRootId);
                }

                aggregateRoot = unitOfWork.GetById(arType, aggregateRootId, null) as T;
                if (aggregateRoot == null)
                {
                    return ReturnAllEventStream(aggregateRootId);
                }
            }*/

            /*   var snapshoot = arType.GetMethod("CreateSnapshot").Invoke(aggregateRoot, new object[0]);
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

               this.myEventStore.Store(uncommitedStream);*/
           /* return new List<AggregateRootEvent>()
                {
                    new AggregateRootEvent(
                        arType.GetMethod("GetSnapshotedStreem").Invoke(aggregateRoot, new object[0]) as CommittedEvent)
                };*/

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
