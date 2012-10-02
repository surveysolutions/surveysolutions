// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClientEventSync.cs" company="">
//   
// </copyright>
// <summary>
//   The client event sync.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Core.CAPI.Synchronization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Denormalizers;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Events;
    using Main.Core.View.CompleteQuestionnaire;

    using Ncqrs;
    using Ncqrs.Eventing;
    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The client event sync.
    /// </summary>
    public class ClientEventSync : AbstractEventSync
    {
        #region Constants and Fields

        /// <summary>
        /// The my event store.
        /// </summary>
        private readonly IEventStore myEventStore;

        /// <summary>
        /// The storage.
        /// </summary>
        private readonly IDenormalizerStorage<CompleteQuestionnaireBrowseItem> storage;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientEventSync"/> class.
        /// </summary>
        /// <param name="storage">
        /// The storage.
        /// </param>
        public ClientEventSync(IDenormalizerStorage<CompleteQuestionnaireBrowseItem> storage)
        {
            this.storage = storage;
            this.myEventStore = NcqrsEnvironment.Get<IEventStore>();
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The read events.
        /// </summary>
        /// <returns>
        /// </returns>
        /// <exception cref="Exception">
        /// </exception>
        public override IEnumerable<AggregateRootEvent> ReadEvents()
        {
            var myEventStore = NcqrsEnvironment.Get<IEventStore>();
            if (myEventStore == null)
            {
                throw new Exception("IEventStore is not correct.");
            }

            var retval = new List<AggregateRootEvent>();
            foreach (CompleteQuestionnaireBrowseItem item in this.storage.Query())
            {
                if (!SurveyStatus.IsStatusAllowCapiSync(item.Status))
                {
                    continue;
                }

                retval.AddRange(this.GetEventStreamById(item.CompleteQuestionnaireId));
            }

            // return retval;
            return retval.OrderBy(x => x.EventTimeStamp);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The get event stream by id.
        /// </summary>
        /// <param name="aggregateRootId">
        /// The aggregate root id.
        /// </param>
        /// <returns>
        /// </returns>
        protected List<AggregateRootEvent> GetEventStreamById(Guid aggregateRootId)
        {
            CommittedEventStream events = this.myEventStore.ReadFrom(aggregateRootId, int.MinValue, int.MaxValue);
            return events.Select(e => new AggregateRootEvent(e)).ToList();
        }

        #endregion
    }
}