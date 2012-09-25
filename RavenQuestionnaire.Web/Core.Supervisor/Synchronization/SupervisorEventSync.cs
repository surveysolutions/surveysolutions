// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SupervisorEventSync.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SupervisorEventSync type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Core.Supervisor.Synchronization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Denormalizers;
    using Main.Core.Documents;
    using Main.Core.Domain;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Events;
    using Main.Core.View.CompleteQuestionnaire;

    using Ncqrs;
    using Ncqrs.Domain;
    using Ncqrs.Eventing;
    using Ncqrs.Eventing.Sourcing.Snapshotting;
    using Ncqrs.Eventing.Storage;
    using Ncqrs.Restoring.EventStapshoot;

    /// <summary>
    /// Responsible for supervisor synchronization
    /// </summary>
    public class SupervisorEventSync : AbstractEventSync
    {
        #region Constants and Fields

        /// <summary>
        /// ViewRepository  object
        /// </summary>
        private readonly IDenormalizer denormalizer;

        /// <summary>
        /// myEventStore object
        /// </summary>
        private readonly IEventStore myEventStore;

        /// <summary>
        /// The unit of work factory.
        /// </summary>
        private readonly IUnitOfWorkFactory unitOfWorkFactory;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SupervisorEventSync"/> class.
        /// </summary>
        /// <param name="denormalizer">
        /// The denormalizer.
        /// </param>
        /// <exception cref="Exception">
        /// added new exception
        /// </exception>
        public SupervisorEventSync(IDenormalizer denormalizer)
        {
            this.denormalizer = denormalizer;
            this.myEventStore = NcqrsEnvironment.Get<IEventStore>();
            this.unitOfWorkFactory = NcqrsEnvironment.Get<IUnitOfWorkFactory>();
            if (this.myEventStore == null)
            {
                throw new Exception("IEventStore is not correct.");
            }
        }

        #endregion

        #region Public Methods and Operators
        /// <summary>
        /// Responsible for reaching eventstream by id
        /// </summary>
        /// <param name="aggregateRootId">
        /// The aggregate root id.
        /// </param>
        /// <param name="arType">
        /// The ar type.
        /// </param>
        /// <returns>
        /// List of aggregate root event
        /// </returns>
        public List<AggregateRootEvent> GetEventStreamById(Guid aggregateRootId, Type arType)
        {
            CommittedEventStream events = this.myEventStore.ReadFrom(aggregateRootId, int.MinValue, int.MaxValue);

            if (!events.Any())
            {
                return new List<AggregateRootEvent>(0);
            }

            IEnumerable<Type> snapshotables = from i in arType.GetInterfaces()
                                              where
                                                  i.IsGenericType
                                                  && i.GetGenericTypeDefinition() == typeof(ISnapshotable<>)
                                              select i;
            if (!snapshotables.Any())
            {
                return this.BuildEventStream(events);
            }

            if (
                !typeof(SnapshootableAggregateRoot<>).MakeGenericType(
                    snapshotables.FirstOrDefault().GetGenericArguments()[0]).IsAssignableFrom(arType))
            {
                return this.BuildEventStream(events);
            }

            if (events.Last().Payload is SnapshootLoaded)
            {
                return new List<AggregateRootEvent> { new AggregateRootEvent(events.Last()) };
            }

            AggregateRoot aggregateRoot;
            using (IUnitOfWorkContext unitOfWork = this.unitOfWorkFactory.CreateUnitOfWork(Guid.NewGuid()))
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

            object snapshoot = arType.GetMethod("CreateSnapshot").Invoke(aggregateRoot, new object[0]);
            var eventSnapshoot = new SnapshootLoaded { Template = new Snapshot(aggregateRootId, 1, snapshoot) };
            Guid commitId = Guid.NewGuid();
            Guid eventId = Guid.NewGuid();
            var uncommitedStream = new UncommittedEventStream(commitId);
            uncommitedStream.Append(
                new UncommittedEvent(
                    eventId, 
                    aggregateRootId, 
                    aggregateRoot.Version + 1, 
                    aggregateRoot.InitialVersion, 
                    DateTime.Now, 
                    eventSnapshoot, 
                    events.Last().GetType().Assembly.GetName().Version));
            this.myEventStore.Store(uncommitedStream);
            return new List<AggregateRootEvent>
                {
                    new AggregateRootEvent(
                        new CommittedEvent(
                        commitId, 
                        eventId, 
                        aggregateRootId, 
                        1, 
                        DateTime.Now, 
                        eventSnapshoot, 
                        events.Last().GetType().Assembly.GetName().Version))
                };
        }

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

        #region Methods

        /// <summary>
        /// Responsible for added init state
        /// </summary>
        /// <param name="retval">
        /// The retval.
        /// </param>
        protected void AddCompleteQuestionnairesInitState(List<AggregateRootEvent> retval)
        {
            IQueryable<CompleteQuestionnaireBrowseItem> model =
                this.denormalizer.Query<CompleteQuestionnaireBrowseItem>();

            foreach (CompleteQuestionnaireBrowseItem item in model)
            {
                if (!SurveyStatus.IsStatusAllowDownSupervisorSync(item.Status))
                {
                    continue;
                }

                retval.AddRange(this.GetEventStreamById(item.CompleteQuestionnaireId, typeof(CompleteQuestionnaireAR)));
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
            IQueryable<FileDescription> model = this.denormalizer.Query<FileDescription>();
            foreach (FileDescription item in model)
            {
                retval.AddRange(this.GetEventStreamById(Guid.Parse(item.PublicKey), typeof(FileAR)));
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
            IQueryable<QuestionnaireDocument> model = this.denormalizer.Query<QuestionnaireDocument>();

            foreach (QuestionnaireDocument item in model)
            {
                retval.AddRange(this.GetEventStreamById(item.PublicKey, typeof(QuestionnaireAR)));
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
            IQueryable<UserDocument> model = this.denormalizer.Query<UserDocument>();
            foreach (UserDocument item in model)
            {
                retval.AddRange(this.GetEventStreamById(item.PublicKey, typeof(UserAR)));
            }
        }

        /// <summary>
        /// The build event stream.
        /// </summary>
        /// <param name="events">
        /// The events.
        /// </param>
        /// <returns>
        /// </returns>
        private List<AggregateRootEvent> BuildEventStream(IEnumerable<CommittedEvent> events)
        {
            return events.Select(e => new AggregateRootEvent(e)).ToList();
        }

        #endregion
    }
}