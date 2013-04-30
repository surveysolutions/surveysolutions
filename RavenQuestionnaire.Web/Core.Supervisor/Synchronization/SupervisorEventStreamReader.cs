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


        //private List<Guid> ARKeys; 


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

            List<Guid> fileNames = GetFiles();

            this.AddFiles(retval, fileNames);
            
            //this.AddRegisterDevice(retval);
            
            return retval.OrderBy(x => x.EventSequence).ToList(); // not good but allows to push correctly
        }

        public override IEnumerable<SyncItemsMeta> GetAllARIds()
        {
            var result = new List<SyncItemsMeta>();

            List<Guid> users = GetUsers();
            result.AddRange(users.Select(i => new SyncItemsMeta(i, "u", GetLastEventFromStream(i))));

            List<Guid> questionnaires = GetQuestionnaires(users);
            result.AddRange(questionnaires.Select(i => new SyncItemsMeta(i, "q", GetLastEventFromStream(i))));

            List<Guid> files = GetFiles();
            result.AddRange(files.Select(i => new SyncItemsMeta(i, "f", GetLastEventFromStream(i))));

            return result;
        }

        public override IEnumerable<AggregateRootEvent> GetARById(Guid ARId, string ARType, Guid? startFrom)
        {
            switch (ARType)
            {
                case "f":
                    return this.GetEventStreamById<FileAR>(ARId);
                    break;
                case "q":
                    return this.GetEventStreamById<CompleteQuestionnaireAR>(ARId);
                    break;
                case "u":
                    return this.GetEventStreamById<UserAR>(ARId);
                    break;
                default:
                    return null;
            }
            //return null;
        }

       

        private List<Guid> GetFiles()
        {
            IQueryable<FileDescription> model = this.denormalizer.Query<FileDescription>();
            
            return model.Select(m => Guid.Parse(m.FileName)).ToList();
        }

        private List<Guid> GetQuestionnaires(List<Guid> users)
        {
            IQueryable<CompleteQuestionnaireBrowseItem> model = 
                this.denormalizer.Query<CompleteQuestionnaireBrowseItem>()
                .Where(q => SurveyStatus.IsStatusAllowDownSupervisorSync(q.Status) && q.Responsible != null && users.Contains(q.Responsible.Id));
            
            return model.Select(i => i.CompleteQuestionnaireId).ToList();
        }

        private List<Guid> GetUsers()
        {
            Guid? supervisorKey = this.GetSupervisor();

            IQueryable<UserDocument> model;

            if (supervisorKey.HasValue)
            {
                model = this.denormalizer.Query<UserDocument>().Where(t => t.Supervisor != null && t.Supervisor.Id == supervisorKey.Value);
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
        /// Retrieves files from db.
        /// </summary>
        /// <param name="retval"></param>
        /// <param name="fileNames"></param>
        protected void AddFiles(List<AggregateRootEvent> retval, List<Guid> fileNames)
        {
            foreach (var item in fileNames)
            {
                retval.AddRange(this.GetEventStreamById<FileAR>(item));
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