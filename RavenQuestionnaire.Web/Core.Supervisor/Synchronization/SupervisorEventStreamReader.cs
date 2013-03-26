// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SupervisorEventStreamReader.cs" company="">
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

    using Main.Core.Documents;
    using Main.Core.Domain;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Events;
    using Main.Core.Events.User;
    using Main.Core.View.CompleteQuestionnaire;
    using Main.Core.View.Questionnaire;
    using Main.DenormalizerStorage;

    using Ncqrs;
    using Ncqrs.Domain;
    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// Responsible for supervisor synchronization
    /// </summary>
    public class SupervisorEventStreamReader : AbstractSnapshotableEventStreamReader
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
        /// The unit of work factory.
        /// </summary>
        private readonly IUnitOfWorkFactory unitOfWorkFactory;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SupervisorEventStreamReader"/> class.
        /// </summary>
        /// <param name="denormalizer">
        /// The denormalizer.
        /// </param>
        /// <exception cref="Exception">
        /// added new exception
        /// </exception>
        public SupervisorEventStreamReader(IDenormalizer denormalizer)
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
        /// Responsible for read events from database
        /// </summary>
        /// <returns>
        /// List of events
        /// </returns>
        public override IEnumerable<AggregateRootEvent> ReadEvents()
        {
            var retval = new List<AggregateRootEvent>();
            Guid? supervisorKey = this.GetSupervisor();
            if (supervisorKey.HasValue)
            {
                this.AddFilteredUsers(retval, supervisorKey.Value);
                this.AddFilteredCompleteQuestionnairesInitState(retval);
            }
            else
            {
                this.AddUsers(retval);
                this.AddCompleteQuestionnairesInitState(retval);
            }

            this.AddQuestionnairesTemplates(retval);

            this.AddFiles(retval);
            this.AddRegisterDevice(retval);
            return retval.OrderBy(x => x.EventSequence).ToList();
        }

        #endregion

        #region Methods

        /// <summary>
        /// The add complete questionnaires init state.
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

                retval.AddRange(this.GetEventStreamById<CompleteQuestionnaireAR>(item.CompleteQuestionnaireId));
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
                retval.AddRange(this.GetEventStreamById<FileAR>(Guid.Parse(item.PublicKey)));
            }
        }

        /// <summary>
        /// The add filtered complete questionnaires initiate state.
        /// </summary>
        /// <param name="retval">
        /// The retval.
        /// </param>
        protected void AddFilteredCompleteQuestionnairesInitState(List<AggregateRootEvent> retval)
        {
            List<Guid> userGuids = retval.Select(rootEvent => (rootEvent.Payload as NewUserCreated).PublicKey).ToList();
            IQueryable<CompleteQuestionnaireBrowseItem> model =
                this.denormalizer.Query<CompleteQuestionnaireBrowseItem>();
            foreach (CompleteQuestionnaireBrowseItem item in model)
            {
                if (SurveyStatus.IsStatusAllowDownSupervisorSync(item.Status) && item.Responsible != null)
                {
                    foreach (Guid guid in userGuids)
                    {
                        if (item.Responsible.Id == guid)
                        {
                            retval.AddRange(
                                this.GetEventStreamById<CompleteQuestionnaireAR>(item.CompleteQuestionnaireId));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The add filtered users.
        /// </summary>
        /// <param name="retval">
        /// The retval.
        /// </param>
        /// <param name="syncKey">
        /// The sync key.
        /// </param>
        protected void AddFilteredUsers(List<AggregateRootEvent> retval, Guid syncKey)
        {
            IQueryable<UserDocument> model =
                this.denormalizer.Query<UserDocument>().Where(t => t.Supervisor != null && t.Supervisor.Id == syncKey);
            foreach (UserDocument item in model)
            {
                retval.AddRange(this.GetEventStreamById<UserAR>(item.PublicKey));
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
            IQueryable<QuestionnaireBrowseItem> model = this.denormalizer.Query<QuestionnaireBrowseItem>();

            foreach (QuestionnaireBrowseItem item in model)
            {
                retval.AddRange(this.GetEventStreamById<QuestionnaireAR>(item.Id));
            }
        }

        /// <summary>
        /// The add register device.
        /// </summary>
        /// <param name="retval">
        /// The retval.
        /// </param>
        protected void AddRegisterDevice(List<AggregateRootEvent> retval)
        {
            IQueryable<SyncDeviceRegisterDocument> model = this.denormalizer.Query<SyncDeviceRegisterDocument>();
            foreach (SyncDeviceRegisterDocument item in model)
            {
                retval.AddRange(this.GetEventStreamById<DeviceAR>(item.PublicKey));
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
                retval.AddRange(this.GetEventStreamById<UserAR>(item.PublicKey));
            }
        }

        /// <summary>
        /// The get supervisor.
        /// </summary>
        /// <returns>
        /// The <see cref="Guid?"/>.
        /// </returns>
        protected Guid? GetSupervisor()
        {
            UserDocument supervisor =
                this.denormalizer.Query<UserDocument>().FirstOrDefault(u => u.Roles.Contains(UserRoles.Supervisor));
            if (supervisor == null)
            {
                return null;
            }

            return supervisor.PublicKey;
        }

        #endregion
    }
}