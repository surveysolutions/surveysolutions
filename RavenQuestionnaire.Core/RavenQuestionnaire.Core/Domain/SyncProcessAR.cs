using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ncqrs.Domain;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Events.Synchronization;

namespace RavenQuestionnaire.Core.Domain
{
    public class SyncProcessAR : AggregateRootMappedByConvention, ISnapshotable<SyncProcessDocument>
    {
        private SyncProcessDocument _innerDocument = new SyncProcessDocument();
        public SyncProcessAR()
        {
        }

        public SyncProcessAR(Guid publicKey, SynchronizationType synckType)
            : base(publicKey)
        {
            ApplyEvent(new NewSynchronizationProcessCreated
                           {
                               ProcessGuid = publicKey,
                               SynckType = synckType
                           });
        }

        protected void OnNewSynchronizationProcessCreated(NewSynchronizationProcessCreated e)
        {
            this._innerDocument = new SyncProcessDocument
                                      {
                                          PublicKey = e.ProcessGuid,
                                          StartDate = DateTime.UtcNow,
                                          Handled = EventState.Initial
                                      };
        }
        public void PushAggregateRootEventStream(IEnumerable<ProcessedEventChunk> aggregateRoots)
        {
            if (this._innerDocument.EndDate.HasValue)
                throw new InvalidOperationException("process is finished, events can't be added");
            ApplyEvent(new AggregateRootEventStreamPushed() {AggregateRoots = aggregateRoots});
        }
        protected void OnPushAggregateRootEventStream(AggregateRootEventStreamPushed e)
        {
            this._innerDocument.Handled = EventState.InProgress;
            this._innerDocument.Chunks = e.AggregateRoots.ToList();
        }
        public void ChangeAggregateRootStatus(Guid eventChunckPublicKey, EventState status)
        {
            if (this._innerDocument.EndDate.HasValue)
                throw new InvalidOperationException("process is finished, events can't be modifyed");
            ApplyEvent(new AggregateRootStatusChanged() { EventChunckPublicKey = eventChunckPublicKey, Status = status });
        }
        protected void OnChangeAggregateRootStatus(AggregateRootStatusChanged e)
        {
           
            var aggregateRoot =
                this._innerDocument.Chunks.FirstOrDefault(d => d.EventChunckPublicKey == e.EventChunckPublicKey);
            if (aggregateRoot == null)
                throw new ArgumentException("Event wasn't find");
            aggregateRoot.Handled = e.Status;
        }
        public void EndProcess(EventState status)
        {
            if (this._innerDocument.EndDate.HasValue)
                throw new InvalidOperationException("process is already finished");

            ApplyEvent(new ProcessEnded() { Status = status });
        }
        protected void OnProcessEnded(ProcessEnded e)
        {
            this._innerDocument.EndDate = DateTime.UtcNow;
            this._innerDocument.Handled = e.Status;
        }

        #region Implementation of ISnapshotable<SyncProcessDocument>

        public SyncProcessDocument CreateSnapshot()
        {
            return this._innerDocument;
        }

        public void RestoreFromSnapshot(SyncProcessDocument snapshot)
        {
            this._innerDocument = snapshot;
        }

        #endregion
    }
}
