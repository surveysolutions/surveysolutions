namespace Main.Core.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Events.Synchronization;
    using Main.Core.View.SyncProcess;

    using Ncqrs.Domain;
    using Ncqrs.Eventing.Sourcing.Snapshotting;

    /// <summary>
    /// The sync process ar.
    /// </summary>
    public class SyncProcessAR : AggregateRootMappedByConvention, ISnapshotable<SyncProcessDocument>
    {
        #region Fields

        /// <summary>
        /// The _inner document.
        /// </summary>
        private SyncProcessDocument innerDocument = new SyncProcessDocument();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncProcessAR"/> class.
        /// </summary>
        public SyncProcessAR()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncProcessAR"/> class.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="synckType">
        /// The synck type.
        /// </param>
        public SyncProcessAR(Guid publicKey, Guid? parentProcessKey, SynchronizationType synckType, string description)
            : base(publicKey)
        {
            this.ApplyEvent(new NewSynchronizationProcessCreated
                {
                    ProcessGuid = publicKey,
                    ParentProcessKey = parentProcessKey,
                    SynckType = synckType, 
                    Description = description
                });
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The change aggregate root status.
        /// </summary>
        /// <param name="eventChunckPublicKey">
        /// The event chunck public key.
        /// </param>
        /// <param name="status">
        /// The status.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Raises InvalidOperationException.
        /// </exception>
        public void ChangeAggregateRootStatus(Guid eventChunckPublicKey, EventState status)
        {
            if (this.innerDocument.EndDate.HasValue)
            {
                throw new InvalidOperationException("process is finished, events can't be modifyed");
            }

            this.ApplyEvent(new AggregateRootStatusChanged { EventChunckPublicKey = eventChunckPublicKey, Status = status });
        }

        /// <summary>
        /// The create snapshot.
        /// </summary>
        /// <returns>
        /// The RavenQuestionnaire.Core.Documents.SyncProcessDocument.
        /// </returns>
        public SyncProcessDocument CreateSnapshot()
        {
            return this.innerDocument;
        }

        /// <summary>
        /// The end process.
        /// </summary>
        /// <param name="status">
        /// The status.
        /// </param>
        /// <param name="description">
        /// The description.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Raises InvalidOperationException.
        /// </exception>
        public void EndProcess(EventState status, string description)
        {
            if (this.innerDocument.EndDate.HasValue)
            {
                throw new InvalidOperationException("process is already finished");
            }

            this.ApplyEvent(new ProcessEnded
                {
                    ProcessKey = this.innerDocument.PublicKey, 
                    Status = status,
                    Description = description
                });
        }

        /// <summary>
        /// Push statistics
        /// </summary>
        /// <param name="statistics">
        /// The statistics.
        /// </param>
        public void PushStatistics(List<UserSyncProcessStatistics> statistics)
        {
            if (this.innerDocument.EndDate.HasValue)
            {
                throw new InvalidOperationException("process is already finished");
            }

            this.ApplyEvent(new ProcessStatisticsCalculated { ProcessKey = this.innerDocument.PublicKey, Statistics = statistics });
        }

        /// <summary>
        /// The push aggregate root event stream.
        /// </summary>
        /// <param name="eventChuncks">
        /// The event chuncks.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Rises InvalidOperationException.
        /// </exception>
        public void PushAggregateRootEventStream(IEnumerable<ProcessedEventChunk> eventChuncks)
        {
            if (this.innerDocument.EndDate.HasValue)
            {
                throw new InvalidOperationException("process is finished, events can't be added");
            }

            this.ApplyEvent(new AggregateRootEventStreamPushed { AggregateRoots = eventChuncks });
        }

        /// <summary>
        /// The restore from snapshot.
        /// </summary>
        /// <param name="snapshot">
        /// The snapshot.
        /// </param>
        public void RestoreFromSnapshot(SyncProcessDocument snapshot)
        {
            this.innerDocument = snapshot;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The on process ended.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnPushStatistics(ProcessStatisticsCalculated e)
        {
            this.innerDocument.Statistics = e.Statistics;
        }

        /// <summary>
        /// The on change aggregate root status.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Raises ArgumentException.
        /// </exception>
        protected void OnChangeAggregateRootStatus(AggregateRootStatusChanged e)
        {
            ProcessedEventChunk aggregateRoot =
                this.innerDocument.Chunks.FirstOrDefault(d => d.EventChunckPublicKey == e.EventChunckPublicKey);
            if (aggregateRoot == null)
            {
                throw new ArgumentException("Event wasn't find");
            }

            aggregateRoot.Handled = e.Status;
        }

        /// <summary>
        /// The on new synchronization process created.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnNewSynchronizationProcessCreated(NewSynchronizationProcessCreated e)
        {
            this.innerDocument = new SyncProcessDocument
                {
                   PublicKey = e.ProcessGuid, 
                   ParentProcessKey = e.ParentProcessKey,
                   StartDate = DateTime.UtcNow, 
                   Handled = EventState.Initial,
                   Description = e.Description
                };
        }

        /// <summary>
        /// The on process ended.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnProcessEnded(ProcessEnded e)
        {
            this.innerDocument.EndDate = DateTime.UtcNow;
            this.innerDocument.Handled = e.Status;
            this.innerDocument.ExitDescription = e.Description;
        }

        /// <summary>
        /// The on push aggregate root event stream.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnPushAggregateRootEventStream(AggregateRootEventStreamPushed e)
        {
            this.innerDocument.Handled = EventState.InProgress;
            this.innerDocument.Chunks = e.AggregateRoots.ToList();
        }
        
        #endregion
    }
}