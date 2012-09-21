// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SupervisorEventSync.cs" company="World Bank">
//   2012
// </copyright>
// <summary>
//   Defines the SupervisorEventSync type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Denormalizers;
using Main.Core.Documents;
using Main.Core.Domain;
using Main.Core.Entities.SubEntities;
using Main.Core.EventHandlers;
using Main.Core.Events;
using Main.Core.View;
using Main.Core.View.CompleteQuestionnaire;
using Main.Core.View.Questionnaire;
using Ncqrs;
using Ncqrs.Domain;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using Ncqrs.Restoring.EventStapshoot;
using Ninject;

namespace Core.Supervisor.Synchronization
{
    /// <summary>
    /// Responsible for supervisor synchronization
    /// </summary>
    public class SupervisorEventSync : AbstractEventSync
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

        private readonly IUnitOfWorkFactory unitOfWorkFactory;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SupervisorEventSync"/> class.
        /// </summary>
        /// <param name="viewRepository">
        /// The view repository.
        /// </param>
        /// <exception cref="Exception">
        /// added new exception
        /// </exception>
        public SupervisorEventSync(IDenormalizer denormalizer)
        {
            this.denormalizer = denormalizer;
            this.myEventStore = NcqrsEnvironment.Get<IEventStore>();
            this.unitOfWorkFactory = NcqrsEnvironment.Get<IUnitOfWorkFactory>();
            if (myEventStore == null)

                throw new Exception("IEventStore is not correct.");
        }

        #endregion

        #region Overrides of AbstractEventSync

        /// <summary>
        /// Responsible for read events from database
        /// </summary>
        /// <returns>
        /// List of events
        /// </returns>
        public override IEnumerable<AggregateRootEvent> ReadEvents()
        {
            var retval = new List<AggregateRootEvent>();
            this.AddCompleteQuestionnairesInitState(retval);
            this.AddQuestionnairesTemplates(retval);
            this.AddUsers(retval);
            this.AddFiles(retval);
            return retval.OrderBy(x => x.EventTimeStamp).ToList();
        }

        #endregion

        #region Protected

        /// <summary>
        /// Responsible for added init state
        /// </summary>
        /// <param name="retval">
        /// The retval.
        /// </param>
        protected void AddCompleteQuestionnairesInitState(List<AggregateRootEvent> retval)
        {
            var model = this.denormalizer.Query<CompleteQuestionnaireBrowseItem>();

            foreach (var item in model)
            {
                if (!SurveyStatus.IsStatusAllowDownSupervisorSync(item.Status))
                    continue;
                retval.AddRange(this.GetEventStreamById(item.CompleteQuestionnaireId, typeof (CompleteQuestionnaireAR)));
            }
        }

        /// <summary>
        /// Responsible for added questionnaire templates
        /// </summary>
        /// <param name="retval">
        /// The retval.
        /// </param>
        protected void AddQuestionnairesTemplates(List<AggregateRootEvent> retval)
        {
            var model = this.denormalizer.Query<QuestionnaireDocument>();

            foreach (var item in model)
            {
                retval.AddRange(this.GetEventStreamById(item.PublicKey, typeof (QuestionnaireAR)));
            }
        }

        /// <summary>
        /// Responsible for load and added users from database
        /// </summary>
        /// <param name="retval">
        /// The retval.
        /// </param>
        protected void AddUsers(List<AggregateRootEvent> retval)
        {
            var model = this.denormalizer.Query<UserDocument>();
            foreach (var item in model)
            {
                retval.AddRange(this.GetEventStreamById(item.PublicKey, typeof (UserAR)));
            }
        }

        /// <summary>
        /// Responsible for upload and added files from database
        /// </summary>
        /// <param name="retval">
        /// The retval.
        /// </param>
        protected void AddFiles(List<AggregateRootEvent> retval)
        {
            var model = this.denormalizer.Query<FileDescription>();
            foreach (var item in model)
            {
                retval.AddRange(this.GetEventStreamById(Guid.Parse(item.PublicKey), typeof (FileAR)));
            }
        }

        public List<AggregateRootEvent> GetEventStreamById(Guid aggregateRootId, Type arType)
            /// Responsible for reaching eventstream by id
            /// </summary>
            /// <param name="retval">
            /// The retval.
            /// </param>
            /// <param name="aggregateRootId">
            /// The aggregate root id.
            /// </param>
        {

            var events = this.myEventStore.ReadFrom(aggregateRootId,
                                                    int.MinValue, int.MaxValue);

            if (!events.Any())
                return new List<AggregateRootEvent>(0);
            var snapshotables = from i in arType.GetInterfaces()
                                where i.IsGenericType && i.GetGenericTypeDefinition() == typeof (ISnapshotable<>)
                                select i;
            if (!snapshotables.Any())
                return BuildEventStream(events);

            if (!typeof (SnapshootableAggregateRoot<>).MakeGenericType(
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
