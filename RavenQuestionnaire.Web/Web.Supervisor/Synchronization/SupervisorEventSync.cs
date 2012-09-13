// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SupervisorEventSync.cs" company="World Bank">
//   2012
// </copyright>
// <summary>
//   Defines the SupervisorEventSync type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Web.Supervisor.Synchronization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ncqrs;
    using Ncqrs.Eventing.Storage;
    using RavenQuestionnaire.Core;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Events;
    using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
    using RavenQuestionnaire.Core.Views.Questionnaire;
    using RavenQuestionnaire.Core.Views.User;

    /// <summary>
    /// Responsible for supervisor synchronization
    /// </summary>
    using Main.Core.Events;

    using RavenQuestionnaire.Core.Views.Event.File;

    public class SupervisorEventSync : AbstractEventSync
    {
        #region Fields

        /// <summary>
        /// ViewRepository  object
        /// </summary>
        private readonly IViewRepository viewRepository;

        /// <summary>
        /// myEventStore object
        /// </summary>
        private readonly IEventStore myEventStore;

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
        public SupervisorEventSync(IViewRepository viewRepository)
        {
            this.viewRepository = viewRepository;
            this.myEventStore = NcqrsEnvironment.Get<IEventStore>();
            if (this.myEventStore == null)
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
            var model = this.viewRepository.Load<CompleteQuestionnaireBrowseInputModel, CompleteQuestionnaireBrowseView>(
                     new CompleteQuestionnaireBrowseInputModel());

            foreach (var item in model.Items)
            {
                if (!SurveyStatus.IsStatusAllowDownSupervisorSync(item.Status))
                    continue;
                GetEventStreamById(retval, item.CompleteQuestionnaireId);
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
            var model = this.viewRepository.Load<QuestionnaireBrowseInputModel, QuestionnaireBrowseView>(
                   new QuestionnaireBrowseInputModel());

            foreach (var item in model.Items)
            {
                this.GetEventStreamById(retval, item.Id);
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
            var model = this.viewRepository.Load<UserBrowseInputModel, UserBrowseView>(new UserBrowseInputModel());
            foreach (var item in model.Items)
            {
                this.GetEventStreamById(retval, item.Id);
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
            var model = this.viewRepository.Load<FileBrowseInputModel, FileBrowseView>(
                   new FileBrowseInputModel());
            foreach (var item in model.Items)
            {
                this.GetEventStreamById(retval, Guid.Parse(item.FileName));
            }
        }

        /// <summary>
        /// Responsible for reaching eventstream by id
        /// </summary>
        /// <param name="retval">
        /// The retval.
        /// </param>
        /// <param name="aggregateRootId">
        /// The aggregate root id.
        /// </param>
        protected void GetEventStreamById(List<AggregateRootEvent> retval, Guid aggregateRootId)
        {
            var events = this.myEventStore.ReadFrom(aggregateRootId, int.MinValue, int.MaxValue);
            retval.AddRange(events.Select(e => new AggregateRootEvent(e)).ToList());
        }

        #endregion
    }
}
