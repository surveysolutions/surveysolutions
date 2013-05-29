// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HqEventStreamReader.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the HqEventStreamReader type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Core.HQ.Synchronization
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
    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The HQ event stream reader.
    /// </summary>
    public class HqEventStreamReader : AbstractSnapshotableEventStreamReader
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

        #endregion

        #region Constructor

        public HqEventStreamReader(IDenormalizer denormalizer)
            : base(denormalizer)
        {
            this.denormalizer = denormalizer;
            this.myEventStore = NcqrsEnvironment.Get<IEventStore>();
            if (myEventStore == null)
                throw new Exception("IEventStore is not correct.");
        }

        #endregion

        #region OverrideMethods

        /// <summary>
        /// The read events.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        public IEnumerable<AggregateRootEvent> ReadAllItems()
        {
            var retval = new List<AggregateRootEvent>();
            this.AddQuestionnairesTemplates(retval);


            this.AddUsersExeptAdmins(retval);
            this.AddCompleteQuestionnairesInitState(retval);
            this.AddFiles(retval);

            return retval.OrderBy(x => x.EventTimeStamp).ToList();
        }

        /// <summary>
        /// The read events.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        public override IEnumerable<AggregateRootEvent> ReadEvents()
        {
            var retval = new List<AggregateRootEvent>();
            this.AddQuestionnairesTemplates(retval);
            
            return retval.OrderBy(x => x.EventTimeStamp).ToList();
        }

        public override IEnumerable<SyncItemsMeta> GetAllARIds()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<AggregateRootEvent> GetARById(Guid ARId, string ARType, Guid? startFrom)
        {
            throw new NotImplementedException();
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
            var model = this.denormalizer.Query<QuestionnaireBrowseItem>();

            foreach (var item in model)
            {
                retval.AddRange(this.GetEventStreamById<QuestionnaireAR>(item.QuestionnaireId));
            }
        }

        /// <summary>
        /// Responsible for load and added users from database
        /// </summary>
        /// <param name="retval">
        /// The retval.
        /// </param>
        protected void AddUsersExeptAdmins(List<AggregateRootEvent> retval)
        {
            IQueryable<UserDocument> model = this.denormalizer.Query<UserDocument>().Where(q => q.Roles != null && !q.Roles.Contains(UserRoles.Administrator));
            foreach (UserDocument item in model)
            {
                retval.AddRange(base.GetEventStreamById<UserAR>(item.PublicKey));
            }
        }

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

                retval.AddRange(base.GetEventStreamById<CompleteQuestionnaireAR>(item.CompleteQuestionnaireId));
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
                retval.AddRange(base.GetEventStreamById<FileAR>(Guid.Parse(item.FileName)));
            }
        }

        #endregion
    }
}
