namespace Core.HQ.Synchronization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Main.Core.Denormalizers;
    using Main.Core.Documents;
    using Main.Core.Domain;
    using Main.Core.Events;
    using Ncqrs;
    using Ncqrs.Domain;
    using Ncqrs.Eventing;
    using Ncqrs.Eventing.Sourcing.Snapshotting;
    using Ncqrs.Eventing.Storage;
    using Ncqrs.Restoring.EventStapshoot;
    
    public class HQEventSync : AbstractEventSync
    {

        #region Fields

        /// <summary>
        /// ViewRepository  object
        /// </summary>
        private readonly IDenormalizer denormalizer;

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

        public HQEventSync(IDenormalizer denormalizer)
        {
            this.denormalizer = denormalizer;
            this.myEventStore = NcqrsEnvironment.Get<IEventStore>();
            this.unitOfWorkFactory = NcqrsEnvironment.Get<IUnitOfWorkFactory>();
            if (myEventStore == null)
                throw new Exception("IEventStore is not correct.");
        }

        #endregion

        #region OverrideMethods

        public override IEnumerable<AggregateRootEvent> ReadEvents()
        {
            var retval = new List<AggregateRootEvent>();
            this.AddQuestionnairesTemplates(retval);
            return retval.OrderBy(x => x.EventTimeStamp).ToList();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Responsible for added questionnaire templates
        /// </summary>
        /// <param name="retval">
        /// The retval.
        /// </param>
        private void AddQuestionnairesTemplates(List<AggregateRootEvent> retval)
        {
            var model = this.denormalizer.Query<QuestionnaireDocument>();

            foreach (var item in model)
            {
                retval.AddRange(this.GetEventStreamById(item.PublicKey, typeof(QuestionnaireAR)));
            }
        }

        private List<AggregateRootEvent> GetEventStreamById(Guid aggregateRootId, Type arType)
        {

            var events = this.myEventStore.ReadFrom(aggregateRootId,
                                                    int.MinValue, int.MaxValue);

            if (!events.Any())
                return new List<AggregateRootEvent>(0);
            var snapshotables = from i in arType.GetInterfaces()
                                where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISnapshotable<>)
                                select i;
            if (!snapshotables.Any())
                return BuildEventStream(events);

            if (!typeof(SnapshootableAggregateRoot<>).MakeGenericType(
                snapshotables.FirstOrDefault().GetGenericArguments()[0]).IsAssignableFrom(arType))
                return BuildEventStream(events);
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
                    return BuildEventStream(events);
                aggregateRoot = unitOfWork.GetById(arType, aggregateRootId, null);
                if (aggregateRoot == null)
                    return BuildEventStream(events);
            }
            var snapshoot = arType.GetMethod("CreateSnapshot").Invoke(aggregateRoot, new object[0]);
            var eventSnapshoot = new SnapshootLoaded()
            {
                Template =
                    new Snapshot(aggregateRootId,
                                 1,
                                 snapshoot)
            };
            Guid commitId = Guid.NewGuid();
            Guid eventId = Guid.NewGuid();
            var uncommitedStream = new UncommittedEventStream(commitId);
            uncommitedStream.Append(new UncommittedEvent(eventId, aggregateRootId, aggregateRoot.Version + 1,
                                                         aggregateRoot.InitialVersion, DateTime.Now, eventSnapshoot,
                                                         events.Last().GetType().Assembly.GetName().Version));
            this.myEventStore.Store(uncommitedStream);
            return new List<AggregateRootEvent>()
                       {

                           new AggregateRootEvent(new CommittedEvent(commitId, eventId, aggregateRootId, 1,
                                                                     DateTime.Now, eventSnapshoot,
                                                                     events.Last().GetType().Assembly.GetName().Version))
                       };

        }

        private List<AggregateRootEvent> BuildEventStream(IEnumerable<CommittedEvent> events)
        {
            return events.Select(e => new AggregateRootEvent(e)).ToList();
        }

        #endregion
    }
}
