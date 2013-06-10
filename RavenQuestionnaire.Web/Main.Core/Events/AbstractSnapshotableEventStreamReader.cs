// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbstractSnapshotableEventStreamReader.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Main.DenormalizerStorage;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Eventing.ServiceModel.Bus;
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

        private readonly IDenormalizer denormalizer;
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

        protected AbstractSnapshotableEventStreamReader(IDenormalizer denormalizer)
        {
            this.denormalizer = denormalizer;
            this.myEventStore = NcqrsEnvironment.Get<IEventStore>();
            if (this.myEventStore == null)
                throw new Exception("IEventStore is not correct.");
            this.commandInvoker = NcqrsEnvironment.Get<ICommandService>();
        }

        #endregion

        protected Guid? GetLastEventFromStream(Guid aggregateRootId)
        {
            var stremableEventStore = myEventStore as IStreamableEventStore;
            if (stremableEventStore != null)
            {
                return stremableEventStore.GetLastEvent(aggregateRootId);
            }
            return null;
        }

        protected List<AggregateRootEvent> ReturnAllEventStream(Guid aggregateRootId)
        {
            var events = this.myEventStore.ReadFrom(aggregateRootId, int.MinValue, int.MaxValue);

            if (!events.Any())
            {
                return new List<AggregateRootEvent>(0);
            }
            return this.BuildEventStream(events);
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
            if (snapshotableEventStore == null)
            {
                return ReturnAllEventStream(aggregateRootId);
            }

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

            this.commandInvoker.Execute(new CreateSnapshotForAR(aggregateRootId, arType));
            //var snapshotLoaded = denormalizer.GetByGuid<CommittedEvent>(aggregateRootId);

            #warning implementation commented out, Nastia said, it it going to be deleted
            return new List<AggregateRootEvent>()
                {
                    //new AggregateRootEvent(snapshotLoaded)
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
