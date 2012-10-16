// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SupervisorEventSync.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SupervisorEventSync type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Main.Core.View.Questionnaire;
using Main.DenormalizerStorage;

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

    using Ncqrs;
    using Ncqrs.Domain;
    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// Responsible for supervisor synchronization
    /// </summary>
    public class SupervisorEventSync : AbstractSnapshotableEventSync
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

                retval.AddRange(base.GetEventStreamById(item.CompleteQuestionnaireId, typeof(CompleteQuestionnaireAR)));
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
                retval.AddRange(base.GetEventStreamById(Guid.Parse(item.PublicKey), typeof(FileAR)));
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
                retval.AddRange(base.GetEventStreamById(item.Id, typeof(QuestionnaireAR)));
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
                retval.AddRange(base.GetEventStreamById(item.PublicKey, typeof(UserAR)));
            }
        }

        #endregion

    }
}