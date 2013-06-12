// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SyncProcessDenormalizer.cs" company="">
//   
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using WB.Core.Infrastructure;

namespace Core.CAPI.Denormalizer
{
    using System;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Events.Synchronization;
    using Main.Core.Events.User;
    using Main.Core.View.CompleteQuestionnaire;
    using Main.Core.View.SyncProcess;
    using Main.DenormalizerStorage;

    using Ncqrs.Eventing.ServiceModel.Bus;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SyncProcessDenormalizer : IEventHandler<NewSynchronizationProcessCreated>, 
                                           IEventHandler<ProcessEnded>,
                                           IEventHandler<ProcessStatisticsCalculated>
    {
        #region Constants and Fields

        /// <summary>
        /// The users.
        /// </summary>
        private readonly IDenormalizerStorage<SyncProcessStatisticsDocument> docs;

        /// <summary>
        /// The surveys
        /// </summary>
        private readonly IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemStore;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncProcessDenormalizer"/> class.
        /// </summary>
        /// <param name="docs">
        /// The docs.
        /// </param>
        /// <param name="documentItemStore">
        /// The document Item Store.
        /// </param>
        public SyncProcessDenormalizer(
            IDenormalizerStorage<SyncProcessStatisticsDocument> docs, 
            IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemStore)
        {
            this.docs = docs;
            this.documentItemStore = documentItemStore;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Start of sync process
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<NewSynchronizationProcessCreated> evnt)
        {
            var stat = new SyncProcessStatisticsDocument(evnt.Payload.ProcessGuid)
                {
                    SyncType = evnt.Payload.SynckType,
                    ParentProcessKey = evnt.Payload.ParentProcessKey,
                    Description = evnt.Payload.Description,
                    CreationDate = evnt.EventTimeStamp
                };
            
            this.docs.Store(stat, stat.PublicKey);
        }

        /// <summary>
        /// Ends sync process
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<ProcessEnded> evnt)
        {
            SyncProcessStatisticsDocument item = this.docs.GetById(evnt.Payload.ProcessKey);
            if (item == null)
            {
                return;
            }

            item.IsEnded = true;
            item.ExitDescription = evnt.Payload.Description;
            item.EndDate = evnt.EventTimeStamp;
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<ProcessStatisticsCalculated> evnt)
        {
        }

        #endregion
    }
}