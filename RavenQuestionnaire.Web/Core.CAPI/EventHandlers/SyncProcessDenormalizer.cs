// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SyncProcessDenormalizer.cs" company="">
//   
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using WB.Core.Infrastructure;

namespace Core.CAPI.EventHandlers
{
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Events.Synchronization;
    using Main.DenormalizerStorage;

    using Ncqrs.Eventing.ServiceModel.Bus;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SyncProcessDenormalizer : IEventHandler<AggregateRootEventStreamPushed>, 
                                           IEventHandler<AggregateRootStatusChanged>, 
                                           IEventHandler<NewSynchronizationProcessCreated>, 
                                           IEventHandler<ProcessEnded>
    {
        #region Constants and Fields

        /// <summary>
        /// The denormalizer.
        /// </summary>
        private readonly IDenormalizerStorage<SyncProcessDocument> denormalizer;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncProcessDenormalizer"/> class.
        /// </summary>
        /// <param name="denormalizer">
        /// The denormalizer.
        /// </param>
        public SyncProcessDenormalizer(IDenormalizerStorage<SyncProcessDocument> denormalizer)
        {
            this.denormalizer = denormalizer;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<AggregateRootEventStreamPushed> evnt)
        {
            SyncProcessDocument process = this.denormalizer.GetById(evnt.EventSourceId);
            if (process == null)
            {
                return;
            }

            process.Chunks = evnt.Payload.AggregateRoots.ToList();
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<AggregateRootStatusChanged> evnt)
        {
            SyncProcessDocument process = this.denormalizer.GetById(evnt.EventSourceId);
            if (process == null)
            {
                return;
            }

            ProcessedEventChunk root =
                process.Chunks.FirstOrDefault(c => c.EventChunckPublicKey == evnt.Payload.EventChunckPublicKey);
            if (root == null)
            {
                return;
            }

            root.Handled = evnt.Payload.Status;
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<NewSynchronizationProcessCreated> evnt)
        {
            this.denormalizer.Store(
                new SyncProcessDocument { PublicKey = evnt.Payload.ProcessGuid, SynckType = evnt.Payload.SynckType }, 
                evnt.Payload.ProcessGuid);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<ProcessEnded> evnt)
        {
            SyncProcessDocument process = this.denormalizer.GetById(evnt.EventSourceId);
            if (process == null)
            {
                return;
            }

            process.Handled = evnt.Payload.Status;
        }

        #endregion
    }
}