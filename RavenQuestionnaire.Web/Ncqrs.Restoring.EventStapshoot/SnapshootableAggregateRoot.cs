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
    public abstract class SnapshootableAggregateRoot<T> : MappedAggregateRoot, ISnapshotable<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SnapshootableAggregateRoot"/> class.
        /// </summary>
        protected SnapshootableAggregateRoot()
            : base(new SnapshootableEventHandlerMappingStrategy(new ConventionBasedEventHandlerMappingStrategy()))
        {
        }

        protected SnapshootableAggregateRoot(Guid id)
            : base(id, new SnapshootableEventHandlerMappingStrategy(new ConventionBasedEventHandlerMappingStrategy()))
        {
        }

        #region Implementation of ISnapshotable<T>

        public abstract T CreateSnapshot();

        public abstract void RestoreFromSnapshot(T snapshot);

        #endregion
        public override void InitializeFromHistory(CommittedEventStream history)
        {
            var lastSnapshoot = history.LastOrDefault(e => e.Payload is SnapshootLoaded);
            if (lastSnapshoot == null)
            {
                base.InitializeFromHistory(history);
                return;
            }
            var newHistory = new CommittedEventStream(history.SourceId,
                                                      history.SkipWhile(e => e != lastSnapshoot).Select(
                                                          (e, i) =>
                                                          new CommittedEvent(e.CommitId, e.EventIdentifier,
                                                                             e.EventSourceId, i + 1, e.EventTimeStamp,
                                                                             e.Payload, e.EventVersion)));
            base.InitializeFromHistory(newHistory);
        }
    }

}
