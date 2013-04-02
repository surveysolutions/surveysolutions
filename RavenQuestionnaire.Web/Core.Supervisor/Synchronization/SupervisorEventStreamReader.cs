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

        private List<Guid> ARKeys; 


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
            this.ARKeys = new List<Guid>();

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

            List<Guid> users = GetUsers();

            this.AddFilteredUsers(retval, users);

            List<Guid> questionnaires = GetQuestionnaires(users);

            this.AddFilteredCompleteQuestionnairesInitState(retval, questionnaires);

            //this.AddQuestionnairesTemplates(retval);

            this.AddFiles(retval);
            
            //this.AddRegisterDevice(retval);
            
            return retval/*OrderBy(x => x.EventSequence).*/.ToList();
        }

        private List<Guid> GetQuestionnaires(List<Guid> users)
        {
            IQueryable<CompleteQuestionnaireBrowseItem> model = 
                this.denormalizer.Query<CompleteQuestionnaireBrowseItem>()
                .Where(q => SurveyStatus.IsStatusAllowDownSupervisorSync(q.Status) && q.Responsible != null && users.Contains(q.Responsible.Id));
            
            return model.Select(i =>i.CompleteQuestionnaireId).ToList();
        }

        private
            List<Guid> GetUsers()
        {
            Guid? supervisorKey = this.GetSupervisor();

            IQueryable<UserDocument> model;

            if (supervisorKey.HasValue)
            {
                model =
                    this.denormalizer.Query<UserDocument>()
                        .Where(t => t.Supervisor != null && t.Supervisor.Id == supervisorKey.Value);
            }
            else
            {
                model = this.denormalizer.Query<UserDocument>();
            }

            return model.Select(u => u.PublicKey).ToList();
            
        }

        #endregion

        #region Methods

        
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
        
        protected void AddFilteredCompleteQuestionnairesInitState(List<AggregateRootEvent> retval, List<Guid> questionnaries)
        {
            foreach (Guid item in questionnaries)
            {
               retval.AddRange(this.GetEventStreamById<CompleteQuestionnaireAR>(item));
            }
        }

       /* /// <summary>
        /// The add complete questionnaires init state.
        /// </summary>
        /// <param name="retval">
        /// The retval.
        /// </param>
        protected void AddCompleteQuestionnairesInitState(List<AggregateRootEvent> retval)
        {
            IQueryable<CompleteQuestionnaireBrowseItem> model = this.denormalizer.Query<CompleteQuestionnaireBrowseItem>();

            foreach (CompleteQuestionnaireBrowseItem item in model)
            {
                if (!SurveyStatus.IsStatusAllowDownSupervisorSync(item.Status))
                {
                    continue;
                }

                 retval.AddRange(
                                this.GetEventStreamById<CompleteQuestionnaireAR>(item.CompleteQuestionnaireId));
            }
        }*/
        
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
        /// The add filtered users.
        /// </summary>
        /// <param name="retval">
        /// The retval.
        /// </param>
        /// <param name="syncKey">
        /// The sync key.
        /// </param>
        protected void AddFilteredUsers(List<AggregateRootEvent> retval, List<Guid> users)
        {
            foreach (var item in users)
            {
                retval.AddRange(this.GetEventStreamById<UserAR>(item));
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
            UserDocument supervisor = this.denormalizer.Query<UserDocument>().FirstOrDefault(u => u.Roles.Contains(UserRoles.Supervisor));
            
            if(supervisor != null)
            {
                return supervisor.PublicKey;
            }

            return null;
        }

        #endregion
    }
}