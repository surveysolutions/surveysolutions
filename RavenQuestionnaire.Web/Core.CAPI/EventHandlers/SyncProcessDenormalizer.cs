// -----------------------------------------------------------------------
// <copyright file="SyncProcessDenormalizer.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Main.Core.Documents;
using Main.Core.Events.Synchronization;
using Main.DenormalizerStorage;
using Ncqrs.Eventing.ServiceModel.Bus;

namespace Core.CAPI.EventHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SyncProcessDenormalizer : IEventHandler<AggregateRootEventStreamPushed>, IEventHandler<AggregateRootStatusChanged>, IEventHandler<NewSynchronizationProcessCreated>, IEventHandler<ProcessEnded>
    {
        private readonly IDenormalizerStorage<SyncProcessDocument> denormalizer;

        public SyncProcessDenormalizer(IDenormalizerStorage<SyncProcessDocument> denormalizer)
        {
            this.denormalizer = denormalizer;
        }

        #region Implementation of IEventHandler<in AggregateRootEventStreamPushed>

        public void Handle(IPublishedEvent<AggregateRootEventStreamPushed> evnt)
        {
            var process = this.denormalizer.GetByGuid(evnt.EventSourceId);
            if (process == null)
                return;
            process.Chunks = evnt.Payload.AggregateRoots.ToList();
        }

        #endregion

        #region Implementation of IEventHandler<in AggregateRootStatusChanged>

        public void Handle(IPublishedEvent<AggregateRootStatusChanged> evnt)
        {
            var process = this.denormalizer.GetByGuid(evnt.EventSourceId);
            if(process==null)
                return;
            var root = process.Chunks.FirstOrDefault(c => c.EventChunckPublicKey == evnt.Payload.EventChunckPublicKey);
            if(root==null)
                return;
            root.Handled = evnt.Payload.Status;
        }

        #endregion

        #region Implementation of IEventHandler<in NewSynchronizationProcessCreated>

        public void Handle(IPublishedEvent<NewSynchronizationProcessCreated> evnt)
        {
            this.denormalizer.Store(
                new SyncProcessDocument() {PublicKey = evnt.Payload.ProcessGuid, SynckType = evnt.Payload.SynckType},
                evnt.Payload.ProcessGuid);
        }

        #endregion

        #region Implementation of IEventHandler<in ProcessEnded>

        public void Handle(IPublishedEvent<ProcessEnded> evnt)
        {
            var process = this.denormalizer.GetByGuid(evnt.EventSourceId);
            if (process == null)
                return;
            process.Handled = evnt.Payload.Status;
        }

        #endregion
    }
}
