// -----------------------------------------------------------------------
// <copyright file="SnapshootableAggregateRoot.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing.Mapping;
using Ncqrs.Eventing.Sourcing.Snapshotting;

namespace Ncqrs.Restoring.EventStapshoot
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public abstract class SnapshootableAggregateRoot<T> : MappedAggregateRoot, ISnapshotable<T> where T:class 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SnapshootableAggregateRoot"/> class.
        /// </summary>
        protected SnapshootableAggregateRoot()
            : base(new ConventionBasedEventHandlerMappingStrategy())
        {
        }

        protected SnapshootableAggregateRoot(Guid id)
            : base(id, new ConventionBasedEventHandlerMappingStrategy())
        {
        }

        public long? LastPersistedSnapshot { get; private set; }
   //     private bool isLastEventSnapshot = false;
        #region Implementation of ISnapshotable<T>

        public abstract T CreateSnapshot();

        public abstract void RestoreFromSnapshot(T snapshot);

        #endregion

        public override void InitializeFromSnapshot(Snapshot snapshot)
            base.InitializeFromSnapshot(snapshot);
            isLastSnapshotSavedToStream = true;
        }
        protected override void ValidateHistoricalEvent(CommittedEvent evnt)
        {
            base.ValidateHistoricalEvent(evnt);
            if (evnt.Payload is SnapshootLoaded)
                throw new InvalidCommittedEventException("event stream can't contain snapshots");
        }
        {
            base.InitializeFromSnapshot(snapshot);
            if (snapshot is CommitedSnapshot)
                LastPersistedSnapshot = snapshot.Version;
        }

      /*  protected override void ValidateHistoricalEvent(CommittedEvent evnt)
        {
            base.ValidateHistoricalEvent(evnt);
            if (evnt.Payload is SnapshootLoaded)
                throw new InvalidCommittedEventException("event stream can't contain snapshots");
        }*/
        protected void OnCreateNewSnapshot(SnapshootLoaded e)
        {
            LastPersistedSnapshot = this.Version;
        }
        public virtual void CreateNewSnapshot()
        {
            if (LastPersistedSnapshot.HasValue && LastPersistedSnapshot.Value==Version)
                return;
            
            var snapshoot = CreateSnapshot();
            var eventSnapshoot = new SnapshootLoaded() { Template = new Snapshot(this.EventSourceId, this.Version + 1, snapshoot) };
          
            ApplyEvent(eventSnapshoot);
           
               if (newHistory.Any())
               {
               }
               else
               {
                   isLastSnapshotSavedToStream = true;
               }*/
        }

        protected void OnCreateNewSnapshot(SnapshootLoaded e)
        {
            /*if(e.Template.Version!=this.Version)
                RestoreFromSnapshot(e.Template.Payload as T);*/
        }
       /* protected override void OnEventApplied(UncommittedEvent appliedEvent)
        {
            base.OnEventApplied(appliedEvent);
            isLastSnapshotSavedToStream = !(appliedEvent.Payload is SnapshootLoaded);
        }*/
        public virtual void CreateNewSnapshot()
        {
            if (isLastSnapshotSavedToStream)
                return;
            var snapshoot = CreateSnapshot();// arType.GetMethod("CreateSnapshot").Invoke(aggregateRoot, new object[0]);
            var eventSnapshoot = new SnapshootLoaded() { Template = new Snapshot(this.EventSourceId, this.Version + 1, snapshoot) };
          /*  Guid commitId = Guid.NewGuid();
            Guid eventId = Guid.NewGuid();
          //  var uncommitedStream = new UncommittedEventStream(commitId);
            var dateOfEvent = NcqrsEnvironment.Get<IClock>().UtcNow();*/

            ApplyEvent(eventSnapshoot);
           /* uncommitedStream.Append(
                new UncommittedEvent(
                    eventId,
                    this.EventSourceId,
                    this.Version + 1,
                    this.InitialVersion,
                    dateOfEvent,
                    eventSnapshoot,
                    GetType().Assembly.GetName().Version));*/
           /* return new CommittedEvent(
                        commitId,
                        eventId,
                        EventSourceId,
                        1,
                        dateOfEvent,
                        eventSnapshoot,
                        this.GetType().Assembly.GetName().Version);*/
            /*this.myEventStore.Store(uncommitedStream);

            return new List<AggregateRootEvent>()
                {
                    new AggregateRootEvent(
                        new CommittedEvent(
                        commitId,
                        eventId,
                        aggregateRootId,
                        1,
                        dateOfEvent,
                        eventSnapshoot,
                        events.Last().GetType().Assembly.GetName().Version))
                       };*/
        }
    }

}
