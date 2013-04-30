// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClientEventStreamReader.cs" company="">
//   
// </copyright>
// <summary>
//   The client event sync.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Main.DenormalizerStorage;

namespace Core.CAPI.Synchronization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Events;
    using Main.Core.View.CompleteQuestionnaire;

    using Ncqrs;
    using Ncqrs.Eventing;
    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The client event sync.
    /// </summary>
    public class ClientEventStreamReader : AbstractEventStreamReader
    {
        #region Constants and Fields

        /// <summary>
        /// The my event store.
        /// </summary>
        private readonly IEventStore eventStore;

        /// <summary>
        /// The storage.
        /// </summary>
        private readonly IDenormalizerStorage<CompleteQuestionnaireBrowseItem> storage;

        /// <summary>
        /// The users
        /// </summary>
        private readonly IDenormalizerStorage<UserDocument> user;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientEventStreamReader"/> class.
        /// </summary>
        /// <param name="storage">
        /// The storage.
        /// </param>
        /// <param name="users">
        /// The users.
        /// </param>
        public ClientEventStreamReader(IDenormalizerStorage<CompleteQuestionnaireBrowseItem> storage, IDenormalizerStorage<UserDocument> users)
        {
            this.user = users;
            this.storage = storage;
            this.eventStore = NcqrsEnvironment.Get<IEventStore>();
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The read events.
        /// </summary>
        /// <param name="syncKey">
        /// The sync Key.
        /// </param>
        /// <returns>
        /// </returns>
        /// <exception cref="Exception">
        /// </exception>
        public override IEnumerable<AggregateRootEvent> ReadEvents()
        {
            if (eventStore == null)
            {
                throw new Exception("IEventStore is not correct.");
            }

            var usersGuid = new List<Guid>();
            var retval = new List<AggregateRootEvent>();

            foreach (var item in this.storage.Query().Where(item => SurveyStatus.IsStatusAllowCapiSync(item.Status)))
            {
                retval.AddRange(this.GetEventStreamById(item.CompleteQuestionnaireId));
            }

            return retval.OrderBy(x => x.EventSequence);
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
            CommittedEventStream events = this.eventStore.ReadFrom(aggregateRootId, int.MinValue, int.MaxValue);
            return events.Select(e => new AggregateRootEvent(e)).ToList();
        }

        /// <summary>
        /// Select user Guid for current supervisor
        /// </summary>
        /// <param name="syncKey">
        /// The sync key.
        /// </param>
        /// <returns>
        /// List of guids
        /// </returns>
        private List<Guid> GetUsersGuid(Guid syncKey)
        {
            return
                this.user.Query().Where(t => t.Supervisor != null && t.Supervisor.Id == syncKey).Select(
                    t => t.PublicKey).ToList();
        }

        #endregion
    }
}