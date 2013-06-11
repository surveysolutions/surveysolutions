// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SupervisorEventStreamReader.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SupervisorEventSync type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using WB.Core.Synchronization;

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

        private readonly Guid supervisorId;
        private readonly bool isApprovedSended;

        //private List<Guid> ARKeys; 


        #endregion

        #region Constructors and Destructors

       
        public SupervisorEventStreamReader(IDenormalizer denormalizer, Guid supervisorId, bool isApprovedSended)
            : base(denormalizer)
        {
            this.denormalizer = denormalizer;
            this.supervisorId = supervisorId;
            this.isApprovedSended = isApprovedSended;
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
            return GetAllARIdsInternal(null);
        }

        public  IEnumerable<SyncItemsMeta> GetAllARIdsInternal(string userName)
        {
            var result = new List<SyncItemsMeta>();

            List<Guid> users = GetUsers();
            result.AddRange(users.Select(i => new SyncItemsMeta(i, SyncItemType.User, null)));

            List<Guid> questionnaires = GetQuestionnaires(users);
            result.AddRange(questionnaires.Select(i => new SyncItemsMeta(i, SyncItemType.Questionnare, null)));

            List<Guid> files = GetFiles();
            result.AddRange(files.Select(i => new SyncItemsMeta(i, SyncItemType.File, null)));

            return result;
        }
        public override IEnumerable<AggregateRootEvent> GetARById(Guid ARId, string ARType, Guid? startFrom)
        {
            switch (ARType)
            {
                case SyncItemType.File:
                    return this.GetEventStreamById(ARId);
                    break;
                case SyncItemType.Questionnare:
                    return this.GetEventStreamById(ARId);
                    break;
                case SyncItemType.User:
                    return this.GetEventStreamById(ARId);
                    break;
                default:
                    return null;
            }
            //return null;
        }

        private List<Guid> GetFiles()
        {
            return this.denormalizer.Query<FileDescription, List<Guid>>(_ => _
                .Select(m => Guid.Parse(m.FileName)).ToList());
        }

        private List<Guid> GetQuestionnaires(List<Guid> users)
        {
            return this.denormalizer.Query<CompleteQuestionnaireBrowseItem, List<Guid>>(_ => _
                                
                .Where(q => IsQuestionnarieRequiresSync(users, q))
                .Select(i => i.CompleteQuestionnaireId)
                .ToList());
        }

        private bool IsQuestionnarieRequiresSync(List<Guid> users, CompleteQuestionnaireBrowseItem q)
        {
            if (q.Status == SurveyStatus.Approve && !isApprovedSended)
                return false;
            return SurveyStatus.IsStatusAllowDownSupervisorSync(q.Status) && q.Responsible != null && users.Contains(q.Responsible.Id);
        }

        private List<Guid> GetUsers()
        {
            return
                 this.denormalizer.Query<UserDocument, List<Guid>>(_ => _
                     .Where(t => t.Supervisor != null && t.Supervisor.Id == supervisorId)
                     .Select(u => u.PublicKey)
                     .ToList());

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
                retval.AddRange(this.GetEventStreamById(item));
            }
        }
        
        protected void AddFilteredCompleteQuestionnairesInitState(List<AggregateRootEvent> retval, List<Guid> questionnaries)
        {
            foreach (Guid item in questionnaries)
            {
               retval.AddRange(this.GetEventStreamById(item));
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
            var model = this.denormalizer.Query<SyncDeviceRegisterDocument, List<AggregateRootEvent>>(_ => _
                .SelectMany(item => this.GetEventStreamById(item.PublicKey))
                .ToList());

            retval.AddRange(model);
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
                retval.AddRange(this.GetEventStreamById(item));
            }
        }


        #endregion
    }
}