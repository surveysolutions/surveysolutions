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
    public abstract class SnapshootableAggregateRoot<T> : MappedAggregateRoot, ISnapshotable<T> where T : class
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
        {
            base.InitializeFromSnapshot(snapshot);
            if (snapshot is CommitedSnapshot)
                LastPersistedSnapshot = snapshot.Version;
        }

        protected void OnCreateNewSnapshot(SnapshootLoaded e)
        {
            RestoreFromSnapshot(e.Template.Payload as T);
            LastPersistedSnapshot = this.Version;
        }

        public virtual void CreateNewSnapshot(T shapshot = null)
        {
            if (ExitWhenSelfSnapshotingWasMadeByPreviousEvent(shapshot))
                return;

            //    var snapshoot = CreateSnapshot();
            var eventSnapshoot = new SnapshootLoaded()
                {
                    Template = new Snapshot(this.EventSourceId, this.Version + 1, shapshot ?? CreateSnapshot())
                };

            ApplyEvent(eventSnapshoot);
        }

        protected bool ExitWhenSelfSnapshotingWasMadeByPreviousEvent(T shapshot)
        {
            return LastPersistedSnapshot.HasValue && LastPersistedSnapshot.Value == Version && shapshot == null;
        }
    }

}
